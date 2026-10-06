"""Dependency-free tests for the Spandrel checkpoint selection logic."""

from __future__ import annotations

import base64
import contextlib
import io
import sys
import tempfile
import types
import unittest
from pathlib import Path
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import spandrel_upscale as runner  # noqa: E402


class FakeDevice:
    def __init__(self, name: str) -> None:
        self.type = name.split(":", 1)[0]

    def __str__(self) -> str:
        return self.type


class FakeNetwork:
    def eval(self) -> None:
        self.evaluated = True


class FakeImageDescriptor:
    def __init__(
        self,
        *,
        purpose: str = "SR",
        scale: int = 4,
        input_channels: int = 3,
        output_channels: int = 3,
        architecture: str = "ExampleSR",
        tags: tuple[str, ...] = (),
    ) -> None:
        self.purpose = purpose
        self.scale = scale
        self.input_channels = input_channels
        self.output_channels = output_channels
        self.architecture = types.SimpleNamespace(id=architecture)
        self.tags = tags
        self.supports_half = False
        self.supports_bfloat16 = False
        self.model = FakeNetwork()
        self.to_args: tuple[object, object] | None = None

    def to(self, device: object, dtype: object) -> FakeImageDescriptor:
        self.to_args = (device, dtype)
        return self


class SpandrelRunnerTests(unittest.TestCase):
    def make_fake_modules(self, models: dict[str, object]):
        torch = types.ModuleType("torch")
        torch.device = FakeDevice
        torch.cuda = types.SimpleNamespace(is_available=lambda: False)
        torch.float32 = object()
        torch.float16 = object()
        torch.bfloat16 = object()

        spandrel = types.ModuleType("spandrel")
        spandrel.ImageModelDescriptor = FakeImageDescriptor

        class ModelLoader:
            def __init__(self, device: object) -> None:
                self.device = device
                self.loaded: list[str] = []

            def load_from_file(self, path: Path) -> object:
                self.loaded.append(path.name)
                value = models[path.name]
                if isinstance(value, Exception):
                    raise value
                return value

        spandrel.ModelLoader = ModelLoader
        return torch, spandrel

    def test_listing_outputs_only_eligible_recognized_models_and_is_depth_limited(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            files = {
                "valid.pth": root / "valid.pth",
                "1x-DXTDecompressor-Source-V3.pth": root / "1x-DXTDecompressor-Source-V3.pth",
                "wrong-scale.pt": root / "wrong-scale.pt",
                "wrong-channels.ckpt": root / "wrong-channels.ckpt",
                "wrong-purpose.safetensors": root / "wrong-purpose.safetensors",
                "not-image.pt": root / "not-image.pt",
                "broken.pth": root / "broken.pth",
                "deep-valid.pt": root / "one" / "two" / "three" / "deep-valid.pt",
                "too-deep.pth": root / "one" / "two" / "three" / "four" / "too-deep.pth",
                runner.PLKSR_MODEL_NAME: root / runner.PLKSR_MODEL_NAME,
                runner.DAT2_MODEL_NAME: root / runner.DAT2_MODEL_NAME,
                "ignored.bin": root / "ignored.bin",
            }
            for path in files.values():
                path.parent.mkdir(parents=True, exist_ok=True)
                path.touch()

            models: dict[str, object] = {
                "valid.pth": FakeImageDescriptor(),
                "1x-DXTDecompressor-Source-V3.pth": FakeImageDescriptor(
                    purpose="Restoration", scale=1, architecture="ESRGAN"
                ),
                "wrong-scale.pt": FakeImageDescriptor(scale=2),
                "wrong-channels.ckpt": FakeImageDescriptor(output_channels=1),
                "wrong-purpose.safetensors": FakeImageDescriptor(purpose="Denoise"),
                "not-image.pt": object(),
                "broken.pth": RuntimeError("invalid checkpoint"),
                "deep-valid.pt": FakeImageDescriptor(architecture="DeepSR"),
                "too-deep.pth": FakeImageDescriptor(),
                runner.PLKSR_MODEL_NAME: FakeImageDescriptor(),
                runner.DAT2_MODEL_NAME: FakeImageDescriptor(),
            }
            torch, spandrel = self.make_fake_modules(models)
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                output = io.StringIO()
                with contextlib.redirect_stdout(output):
                    result = runner.list_supported_models(root)

            self.assertEqual(result, 0)
            emitted = []
            details_by_path = {}
            for line in output.getvalue().splitlines():
                self.assertTrue(line.startswith("MODEL:"))
                model_fields = line.removeprefix("MODEL:").split("\t")
                model_path = base64.b64decode(model_fields[0]).decode("utf-8")
                emitted.append(model_path)
                details_by_path[model_path] = model_fields[1:]
            self.assertEqual(
                emitted,
                [
                    str(files["1x-DXTDecompressor-Source-V3.pth"].resolve()),
                    str(files[runner.PLKSR_MODEL_NAME].resolve()),
                    str(files[runner.DAT2_MODEL_NAME].resolve()),
                    str(files["deep-valid.pt"].resolve()),
                    str(files["valid.pth"].resolve()),
                ],
            )
            dxt_details = details_by_path[str(files["1x-DXTDecompressor-Source-V3.pth"].resolve())]
            self.assertEqual(base64.b64decode(dxt_details[0]).decode("utf-8"), "ESRGAN")
            self.assertEqual(dxt_details[1], "1")
            self.assertEqual(base64.b64decode(dxt_details[2]).decode("utf-8"), "Restoration")
            self.assertEqual(dxt_details[3:], ["3", "3"])

    def test_listing_requires_an_existing_model_folder(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            missing = Path(temporary) / "missing"
            with self.assertRaises(NotADirectoryError):
                runner.list_supported_models(missing)

    def test_generic_loading_accepts_unknown_names_but_still_validates_scale_and_channels(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "my-custom-model.pth"
            checkpoint.touch()
            descriptor = FakeImageDescriptor(architecture="SomeRegisteredSR")
            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                with self.assertRaisesRegex(ValueError, "supports only"):
                    runner._load_model(checkpoint, force_cpu=True)
                loaded, device, dtype, device_name = runner._load_model(
                    checkpoint, force_cpu=True, generic_model=True
                )

            self.assertIs(loaded, descriptor)
            self.assertEqual(device.type, "cpu")
            self.assertEqual(device_name, "cpu")
            self.assertEqual(descriptor.to_args, (device, dtype))
            self.assertTrue(descriptor.model.evaluated)

    def test_generic_loading_accepts_1x_dxt_decompressor(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "1x-DXTDecompressor-Source-V3.pth"
            checkpoint.touch()
            descriptor = FakeImageDescriptor(
                purpose="Restoration", scale=1, architecture="ESRGAN"
            )
            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                loaded, _, _, _ = runner._load_model(
                    checkpoint, force_cpu=True, generic_model=True
                )
            self.assertIs(loaded, descriptor)

    def test_generic_loading_rejects_unsupported_scales(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "two-x.pth"
            checkpoint.touch()
            torch, spandrel = self.make_fake_modules(
                {checkpoint.name: FakeImageDescriptor(scale=2)}
            )
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                with self.assertRaisesRegex(
                    ValueError, "1x RGB restoration or 4x RGB super-resolution"
                ):
                    runner._load_model(checkpoint, force_cpu=True, generic_model=True)

    def test_list_models_cli_does_not_require_inference_arguments(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            with patch.object(
                sys,
                "argv",
                ["spandrel_upscale.py", "--list-models", temporary],
            ):
                args = runner.parse_args()
            self.assertEqual(args.list_models, Path(temporary))
            self.assertIsNone(args.model)
            self.assertIsNone(args.input)
            self.assertIsNone(args.output)


if __name__ == "__main__":
    unittest.main()
