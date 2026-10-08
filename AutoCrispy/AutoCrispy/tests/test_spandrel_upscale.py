"""Tests for the Spandrel checkpoint selection and image post-processing logic."""

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

import numpy as np
from PIL import Image

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
        tiling: str = "SUPPORTED",
        supports_half: bool = False,
    ) -> None:
        self.purpose = purpose
        self.scale = scale
        self.input_channels = input_channels
        self.output_channels = output_channels
        self.architecture = types.SimpleNamespace(id=architecture)
        self.tags = tags
        self.supports_half = supports_half
        self.supports_bfloat16 = False
        self.tiling = types.SimpleNamespace(name=tiling)
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
        torch.configured_num_threads = None
        torch.configured_interop_threads = None
        torch.set_num_threads = lambda count: setattr(torch, "configured_num_threads", count)
        torch.set_num_interop_threads = lambda count: setattr(torch, "configured_interop_threads", count)
        torch.backends = types.SimpleNamespace(
            cudnn=types.SimpleNamespace(benchmark=False, allow_tf32=False),
            cuda=types.SimpleNamespace(
                matmul=types.SimpleNamespace(allow_tf32=False)
            ),
        )

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

    def test_normal_map_xyz_mode_renormalizes_vectors_and_handles_zero_length(self) -> None:
        source = np.asarray(
            [[[0.75, 0.625, 0.875], [0.1, 0.3, 0.8], [0.5, 0.5, 0.5]]],
            dtype=np.float32,
        )
        output = runner._postprocess_normal_map(source, "normalize-xyz")
        decoded = output * 2.0 - 1.0

        np.testing.assert_allclose(np.linalg.norm(decoded, axis=2), 1.0, atol=1e-6)
        source_vector = source[0, 0] * 2.0 - 1.0
        expected_direction = source_vector / np.linalg.norm(source_vector)
        np.testing.assert_allclose(decoded[0, 0], expected_direction, atol=1e-6)
        np.testing.assert_array_equal(output[0, 2], [0.5, 0.5, 1.0])

    def test_normal_map_rebuild_z_uses_xy_and_clamps_invalid_vectors(self) -> None:
        source = np.asarray(
            [[[0.5, 0.5, 0.0], [1.0, 0.5, 0.0], [1.0, 1.0, 0.0]]],
            dtype=np.float32,
        )
        output = runner._postprocess_normal_map(source, "rebuild-z")
        decoded = output * 2.0 - 1.0

        np.testing.assert_allclose(np.linalg.norm(decoded, axis=2), 1.0, atol=1e-6)
        np.testing.assert_allclose(decoded[0, :, 2], [1.0, 0.0, 0.0], atol=1e-6)
        self.assertTrue(bool(np.all(decoded[0, :, 2] >= 0.0)))

    def test_normal_map_none_is_unchanged_and_unknown_modes_are_rejected(self) -> None:
        source = np.zeros((1, 1, 3), dtype=np.float32)
        self.assertIs(runner._postprocess_normal_map(source, "none"), source)
        with self.assertRaisesRegex(ValueError, "Unsupported normal-map mode"):
            runner._postprocess_normal_map(source, "auto")

    def test_normal_map_mode_preserves_alpha_during_image_save(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            input_path = root / "input.png"
            output_path = root / "output.png"
            source = Image.new("RGBA", (2, 1))
            source.putdata([(128, 128, 0, 37), (255, 128, 0, 211)])
            source.save(input_path)
            upscaled = np.asarray(
                [[[0.5, 0.5, 0.0], [1.0, 0.5, 0.0]]], dtype=np.float32
            )

            with patch.object(runner, "_upscale_with_fallback", return_value=upscaled):
                runner._process_image(
                    input_path,
                    output_path,
                    types.SimpleNamespace(scale=1),
                    FakeDevice("cpu"),
                    "float32",
                    0,
                    "rebuild-z",
                )

            with Image.open(output_path) as result:
                self.assertEqual(result.mode, "RGBA")
                self.assertEqual(result.getpixel((0, 0)), (128, 128, 255, 37))
                self.assertEqual(result.getpixel((1, 0)), (255, 128, 128, 211))

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
                "valid.pth": FakeImageDescriptor(tiling="INTERNAL", supports_half=True),
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
            self.assertEqual(dxt_details[3:], ["3", "3", "SUPPORTED", "false"])
            valid_details = details_by_path[str(files["valid.pth"].resolve())]
            self.assertEqual(valid_details[5:], ["INTERNAL", "true"])

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
            self.assertEqual(torch.configured_num_threads, runner._resolve_cpu_threads())
            self.assertEqual(torch.configured_interop_threads, 1)
            self.assertTrue(descriptor.model.evaluated)

    def test_cpu_thread_auto_uses_up_to_16_and_honors_an_override(self) -> None:
        with patch.object(runner.os, "cpu_count", return_value=32):
            self.assertEqual(runner._resolve_cpu_threads(), 16)
            self.assertEqual(runner._resolve_cpu_threads(8), 8)
        with patch.object(runner.os, "cpu_count", return_value=4):
            self.assertEqual(runner._resolve_cpu_threads(), 4)

    def test_internal_tiling_metadata_bypasses_external_tiles(self) -> None:
        model = types.SimpleNamespace(tiling=types.SimpleNamespace(name="INTERNAL"))
        device = FakeDevice("cuda:0")
        dtype = object()
        image = object()
        result = object()
        fake_torch = types.ModuleType("torch")
        with (
            patch.dict(sys.modules, {"torch": fake_torch}),
            patch.object(runner, "_infer_tile", return_value=result) as infer_tile,
            patch.object(runner, "_upscale_rgb") as upscale_rgb,
        ):
            actual = runner._upscale_with_fallback(
                image, model, device, dtype, tile_size=512
            )

        self.assertIs(actual, result)
        infer_tile.assert_called_once_with(model, image, device, dtype)
        upscale_rgb.assert_not_called()

    def test_internal_tiling_metadata_does_not_trigger_external_oom_retry(self) -> None:
        model = types.SimpleNamespace(tiling=types.SimpleNamespace(name="INTERNAL"))
        device = FakeDevice("cuda:0")
        fake_torch = types.ModuleType("torch")
        with (
            patch.dict(sys.modules, {"torch": fake_torch}),
            patch.object(
                runner, "_infer_tile", side_effect=RuntimeError("CUDA out of memory")
            ),
            patch.object(runner, "_upscale_rgb") as upscale_rgb,
        ):
            with self.assertRaisesRegex(RuntimeError, "out of memory"):
                runner._upscale_with_fallback(
                    object(), model, device, object(), tile_size=512
                )

        upscale_rgb.assert_not_called()

    def test_discouraged_tiling_warns_but_keeps_external_tiling(self) -> None:
        model = types.SimpleNamespace(tiling=types.SimpleNamespace(name="DISCOURAGED"))
        device = FakeDevice("cuda:0")
        dtype = object()
        image = object()
        result = object()
        fake_torch = types.ModuleType("torch")
        output = io.StringIO()
        with contextlib.redirect_stdout(output):
            runner._report_model_tiling_metadata(model, "Test model")
        with (
            patch.dict(sys.modules, {"torch": fake_torch}),
            patch.object(runner, "_upscale_rgb", return_value=result) as upscale_rgb,
        ):
            actual = runner._upscale_with_fallback(
                image, model, device, dtype, tile_size=512
            )

        self.assertIn("discourages external tiling", output.getvalue())
        self.assertIs(actual, result)
        upscale_rgb.assert_called_once_with(image, model, device, dtype, 512)

    def test_cuda_runtime_enables_cudnn_benchmark_and_tf32(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "fast-model.pth"
            checkpoint.touch()
            descriptor = FakeImageDescriptor(architecture="SomeRegisteredSR")
            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            torch.cuda = types.SimpleNamespace(is_available=lambda: True)
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                runner._load_model(checkpoint, force_cpu=False, generic_model=True)

            self.assertTrue(torch.backends.cudnn.benchmark)
            self.assertTrue(torch.backends.cudnn.allow_tf32)
            self.assertTrue(torch.backends.cuda.matmul.allow_tf32)

    def test_auto_precision_uses_fp16_when_cuda_checkpoint_supports_it(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "half-model.pth"
            checkpoint.touch()
            descriptor = FakeImageDescriptor(architecture="SomeRegisteredSR")
            descriptor.supports_half = True
            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            torch.cuda = types.SimpleNamespace(is_available=lambda: True)
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                _, device, dtype, _ = runner._load_model(
                    checkpoint, force_cpu=False, generic_model=True
                )

            self.assertEqual(device.type, "cuda")
            self.assertIs(dtype, torch.float16)

    def test_explicit_fp16_uses_fp16_when_cuda_checkpoint_supports_it(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "half-model.pth"
            checkpoint.touch()
            descriptor = FakeImageDescriptor(architecture="SomeRegisteredSR", supports_half=True)
            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            torch.cuda = types.SimpleNamespace(is_available=lambda: True)
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                _, device, dtype, _ = runner._load_model(
                    checkpoint, force_cpu=False, generic_model=True, precision="fp16"
                )

            self.assertEqual(device.type, "cuda")
            self.assertIs(dtype, torch.float16)

    def test_explicit_fp16_rejects_cpu_and_unsupported_checkpoints(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "no-half.pth"
            checkpoint.touch()
            descriptor = FakeImageDescriptor(architecture="SomeRegisteredSR")
            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                with self.assertRaisesRegex(ValueError, "requires CUDA"):
                    runner._load_model(
                        checkpoint, force_cpu=True, generic_model=True, precision="fp16"
                    )

            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            torch.cuda = types.SimpleNamespace(is_available=lambda: True)
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                with self.assertRaisesRegex(ValueError, "does not advertise FP16 support"):
                    runner._load_model(
                        checkpoint, force_cpu=False, generic_model=True, precision="fp16"
                    )

    def test_explicit_fp32_disables_tf32_on_cuda(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "half-capable.pth"
            checkpoint.touch()
            descriptor = FakeImageDescriptor(architecture="SomeRegisteredSR")
            descriptor.supports_half = True
            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            torch.cuda = types.SimpleNamespace(is_available=lambda: True)
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                _, _, dtype, _ = runner._load_model(
                    checkpoint,
                    force_cpu=False,
                    generic_model=True,
                    precision="fp32",
                )

            self.assertIs(dtype, torch.float32)
            self.assertFalse(torch.backends.cudnn.allow_tf32)
            self.assertFalse(torch.backends.cuda.matmul.allow_tf32)

            runner._configure_torch_runtime(torch, FakeDevice("cuda:0"), precision="auto")
            self.assertTrue(torch.backends.cudnn.allow_tf32)
            self.assertTrue(torch.backends.cuda.matmul.allow_tf32)

    def test_cuda_uses_float32_when_checkpoint_only_advertises_bfloat16(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            checkpoint = Path(temporary) / "bf16-model.pth"
            checkpoint.touch()
            descriptor = FakeImageDescriptor(architecture="SomeRegisteredSR")
            descriptor.supports_bfloat16 = True
            torch, spandrel = self.make_fake_modules({checkpoint.name: descriptor})
            torch.cuda = types.SimpleNamespace(
                is_available=lambda: True,
                is_bf16_supported=lambda: True,
            )
            with patch.dict(sys.modules, {"torch": torch, "spandrel": spandrel}):
                loaded, device, dtype, _ = runner._load_model(
                    checkpoint, force_cpu=False, generic_model=True
                )

            self.assertIs(loaded, descriptor)
            self.assertEqual(device.type, "cuda")
            self.assertIs(dtype, torch.float32)
            self.assertEqual(descriptor.to_args, (device, torch.float32))

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

    def test_auto_route_logs_selected_models_and_completed_roles(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            input_folder = root / "input"
            output_folder = input_folder / "processed"
            nested_folder = input_folder / "nested"
            nested_folder.mkdir(parents=True)
            output_folder.mkdir()
            stale_output = output_folder / "old-result.png"
            stale_output.touch()
            architect_file = input_folder / "a-architect.png"
            painter_file = input_folder / "z-painter.png"
            nested_file = nested_folder / "n-architect.png"
            unsupported_file = nested_folder / "ignored.dds"
            architect_file.touch()
            painter_file.touch()
            nested_file.touch()
            unsupported_file.touch()
            architect_path = root / "solid-textures-model.pth"
            painter_path = root / "repeating-textures-model.safetensors"
            architect = FakeImageDescriptor()
            painter = FakeImageDescriptor()
            args = types.SimpleNamespace(
                input=input_folder,
                output=output_folder,
                architect_model=architect_path,
                painter_model=painter_path,
                cpu=False,
                tile_size=512,
                painter_share=100,
                debug=False,
            )

            def fake_load_model(
                path: Path,
                _force_cpu: bool,
                generic_model: bool,
                cpu_threads: int = 0,
                precision: str = "auto",
            ):
                model = architect if path == architect_path else painter
                return model, FakeDevice("cuda:0"), "float32", "cuda:0"

            def fake_features(path: Path) -> dict[str, float]:
                return {"score": 0.5 if path == painter_file else 0.1}

            processed_files: list[Path] = []
            processed_outputs: dict[Path, Path] = {}

            def fake_process_image(input_path: Path, output_path: Path, *_args: object) -> None:
                processed_files.append(input_path)
                processed_outputs[input_path] = output_path

            output = io.StringIO()
            with (
                patch.object(runner, "_load_model", side_effect=fake_load_model),
                patch.object(runner, "_texture_features", side_effect=fake_features),
                patch.object(runner, "_process_image", side_effect=fake_process_image),
                contextlib.redirect_stdout(output),
            ):
                result = runner._run_auto_route(args)

            self.assertEqual(result, 0)
            self.assertCountEqual(processed_files, [architect_file, painter_file, nested_file])
            self.assertNotIn(unsupported_file, processed_files)
            self.assertNotIn(stale_output, processed_files)
            self.assertEqual(processed_outputs[nested_file], output_folder / "nested" / nested_file.name)
            self.assertEqual(processed_outputs[architect_file], output_folder / architect_file.name)
            log = output.getvalue()
            self.assertIn("AUTOCRISPY_ROUTE_SUMMARY: total=3; Architect=2; Painter=1", log)
            self.assertIn(f"AUTOCRISPY_MODELS: Architect={architect_path.resolve()}", log)
            self.assertIn(f"Painter={painter_path.resolve()}", log)
            self.assertIn("AUTOCRISPY_RESULT: 1/3 · Architect · a-architect.png · OK", log)
            self.assertIn("AUTOCRISPY_RESULT: 3/3 · Painter · z-painter.png · OK", log)
            completed_names = {
                base64.b64decode(line.split(":", 1)[1].strip()).decode("utf-8")
                for line in log.splitlines()
                if line.startswith("AUTOCRISPY_TEXTURE_COMPLETED:")
            }
            self.assertEqual(completed_names, {"a-architect.png", "n-architect.png", "z-painter.png"})

    def test_single_model_runner_recursively_preserves_relative_output_paths(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            input_folder = root / "input"
            nested_folder = input_folder / "world" / "textures"
            nested_folder.mkdir(parents=True)
            nested_file = nested_folder / "stone.png"
            nested_file.touch()
            output_folder = input_folder / "processed"
            output_folder.mkdir()
            stale_output = output_folder / "old-result.png"
            stale_output.touch()
            model_file = root / "model.pth"
            args = types.SimpleNamespace(
                preview_route=False,
                auto_route=False,
                model=model_file,
                input=input_folder,
                output=output_folder,
                cpu=True,
                generic_model=True,
                tile_size=0,
            )
            descriptor = FakeImageDescriptor()
            outputs: list[tuple[Path, Path]] = []
            output = io.StringIO()
            with (
                patch.object(
                    runner,
                    "_load_model",
                    return_value=(descriptor, FakeDevice("cpu"), "float32", "cpu"),
                ),
                patch.object(
                    runner,
                    "_process_image",
                    side_effect=lambda source, destination, *_args: outputs.append((source, destination)),
                ),
                contextlib.redirect_stdout(output),
            ):
                result = runner.run(args)

            self.assertEqual(result, 0)
            self.assertEqual(outputs, [(nested_file, output_folder / "world" / "textures" / "stone.png")])
            self.assertNotIn(stale_output, [source for source, _ in outputs])
            self.assertIn("AUTOCRISPY_PROGRESS: 1/1 · Upscaling · stone.png", output.getvalue())
            self.assertIn("AUTOCRISPY_RESULT: 1/1 · Upscaling · stone.png · OK", output.getvalue())
            completed_line = next(
                line for line in output.getvalue().splitlines()
                if line.startswith("AUTOCRISPY_TEXTURE_COMPLETED:")
            )
            self.assertEqual(
                base64.b64decode(completed_line.split(":", 1)[1].strip()).decode("utf-8"),
                "stone.png",
            )

    def test_image_discovery_keeps_images_when_output_is_a_parent_folder(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            output_folder = Path(temporary)
            input_folder = output_folder / "input"
            input_folder.mkdir()
            image = input_folder / "texture.png"
            image.touch()

            self.assertEqual(runner._iter_supported_images(input_folder, output_folder), [image])

    def test_single_model_runner_rejects_identical_input_and_output(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            input_folder = Path(temporary) / "input"
            input_folder.mkdir()
            args = types.SimpleNamespace(
                preview_route=False,
                auto_route=False,
                model=Path(temporary) / "model.pth",
                input=input_folder,
                output=input_folder,
                cpu=True,
                generic_model=True,
                tile_size=0,
            )
            with patch.object(runner, "_load_model") as load_model:
                with self.assertRaisesRegex(ValueError, "must be different"):
                    runner.run(args)
            load_model.assert_not_called()

    def test_single_model_runner_skips_model_loading_for_empty_input(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            input_folder = root / "input"
            input_folder.mkdir()
            args = types.SimpleNamespace(
                preview_route=False,
                auto_route=False,
                model=root / "model.pth",
                input=input_folder,
                output=root / "output",
                cpu=True,
                generic_model=True,
                tile_size=0,
            )
            output = io.StringIO()
            with patch.object(runner, "_load_model") as load_model, contextlib.redirect_stdout(output):
                result = runner.run(args)
            self.assertEqual(result, 0)
            self.assertIn("No supported images found.", output.getvalue())
            load_model.assert_not_called()

    def test_single_checkpoint_cli_does_not_enable_auto_route(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            model_path = root / "4x-PBRify_UpscalerV4.pth"
            with patch.object(
                sys,
                "argv",
                [
                    "spandrel_upscale.py",
                    str(model_path),
                    "--input",
                    str(root / "input"),
                    "--output",
                    str(root / "output"),
                    "--generic-model",
                ],
            ):
                args = runner.parse_args()
            self.assertFalse(args.auto_route)
            self.assertEqual(args.tile_size, 1024)
            self.assertEqual(args.cpu_threads, 0)
            self.assertEqual(args.precision, "auto")
            self.assertEqual(args.normal_map_mode, "none")
            self.assertEqual(args.model, model_path)
            self.assertIsNone(args.architect_model)
            self.assertIsNone(args.painter_model)

    def test_cli_accepts_explicit_fp32_precision(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            with patch.object(
                sys,
                "argv",
                [
                    "spandrel_upscale.py",
                    str(root / "model.pth"),
                    "--input",
                    str(root / "input"),
                    "--output",
                    str(root / "output"),
                    "--precision",
                    "fp32",
                ],
            ):
                args = runner.parse_args()
            self.assertEqual(args.precision, "fp32")

    def test_cli_accepts_rebuilding_normal_map_z(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            with patch.object(
                sys,
                "argv",
                [
                    "spandrel_upscale.py",
                    str(root / "model.pth"),
                    "--input",
                    str(root / "input"),
                    "--output",
                    str(root / "output"),
                    "--normal-map-mode",
                    "rebuild-z",
                ],
            ):
                args = runner.parse_args()
            self.assertEqual(args.normal_map_mode, "rebuild-z")

    def test_preview_route_cli_needs_only_an_input_folder(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            with patch.object(
                sys,
                "argv",
                [
                    "spandrel_upscale.py",
                    "--preview-route",
                    "--input",
                    str(root),
                    "--painter-share",
                    "45",
                    "--painter-threshold",
                    "0.27",
                    "--preview-limit",
                    "7",
                ],
            ):
                args = runner.parse_args()
            self.assertTrue(args.preview_route)
            self.assertIsNone(args.output)
            self.assertEqual(args.painter_share, 45)
            self.assertAlmostEqual(args.painter_threshold, 0.27)
            self.assertEqual(args.preview_limit, 7)

    def test_preview_route_handles_folders_without_supported_images(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            args = types.SimpleNamespace(
                input=Path(temporary),
                painter_share=30,
                painter_threshold=0.34,
            )
            output = io.StringIO()
            with contextlib.redirect_stdout(output):
                result = runner._preview_auto_route(args)

            self.assertEqual(result, 0)
            self.assertIn("total=0; Architect=0; Painter=0", output.getvalue())
            self.assertIn("Sample=all 0", output.getvalue())

    def test_preview_route_reports_feature_scores_and_assignments_without_loading_models(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            input_folder = Path(temporary)
            low_score = input_folder / "architect.png"
            high_score = input_folder / "painter.png"
            low_score.touch()
            high_score.touch()
            args = types.SimpleNamespace(
                input=input_folder,
                painter_share=30,
                painter_threshold=0.34,
            )
            features = {
                low_score: {
                    "score": 0.12,
                    "detail": 0.1,
                    "edge_density": 0.2,
                    "orientation_entropy": 0.3,
                    "local_pattern_entropy": 0.4,
                    "periodicity": 0.5,
                },
                high_score: {
                    "score": 0.72,
                    "detail": 0.7,
                    "edge_density": 0.6,
                    "orientation_entropy": 0.5,
                    "local_pattern_entropy": 0.8,
                    "periodicity": 0.9,
                },
            }
            output = io.StringIO()
            with (
                patch.object(runner, "_texture_features", side_effect=lambda path: features[path]),
                contextlib.redirect_stdout(output),
            ):
                result = runner._preview_auto_route(args)

            self.assertEqual(result, 0)
            log = output.getvalue()
            self.assertIn("AUTOCRISPY_ROUTE_PREVIEW_SUMMARY: total=2; Architect=2; Painter=0", log)
            self.assertIn("AUTOCRISPY_ROUTE_PREVIEW_ITEM:", log)
            self.assertIn("\tArchitect\t0.720000\t", log)
            self.assertIn("\tArchitect\t0.120000\t", log)
            self.assertIn("Eligible, but outside the Painter share cap", log)
            self.assertIn("Below minimum Painter score", log)
            self.assertIn("Sample=all 2 (sorted by path)", log)

    def test_preview_limit_scores_first_sorted_supported_images_recursively(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            input_folder = Path(temporary) / "input"
            nested_folder = input_folder / "b-nested"
            nested_folder.mkdir(parents=True)
            first = input_folder / "a.png"
            second = nested_folder / "b.png"
            third = input_folder / "c.png"
            unsupported = nested_folder / "ignored.dds"
            for path in (first, second, third, unsupported):
                path.touch()

            args = types.SimpleNamespace(
                input=input_folder,
                painter_share=30,
                painter_threshold=0.34,
                preview_limit=2,
            )
            analyzed: list[Path] = []
            features = {
                "score": 0.1,
                "detail": 0.1,
                "edge_density": 0.1,
                "orientation_entropy": 0.1,
                "local_pattern_entropy": 0.1,
                "periodicity": 0.1,
            }
            output = io.StringIO()
            with (
                patch.object(
                    runner,
                    "_texture_features",
                    side_effect=lambda path: analyzed.append(path) or features,
                ),
                contextlib.redirect_stdout(output),
            ):
                result = runner._preview_auto_route(args)

            self.assertEqual(result, 0)
            self.assertCountEqual(analyzed, [first, second])
            self.assertNotIn(third, analyzed)
            self.assertNotIn(unsupported, analyzed)
            self.assertIn("total=2; Architect=2; Painter=0", output.getvalue())
            self.assertIn("Sample=first 2 of 3 (sorted by path)", output.getvalue())

    def test_painter_threshold_filters_low_scoring_candidates(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            scored = [
                (root / "low.png", {"score": 0.30}),
                (root / "borderline.png", {"score": 0.45}),
                (root / "high.png", {"score": 0.82}),
            ]
            selected = runner._select_painter_files(scored, painter_share=100, painter_threshold=0.5)
            self.assertEqual(selected, {root / "high.png"})

    def test_auto_route_cli_accepts_two_models_without_a_single_model_argument(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            with patch.object(
                sys,
                "argv",
                [
                    "spandrel_upscale.py",
                    "--auto-route",
                    "--architect-model",
                    str(root / "solid-surfaces.safetensors"),
                    "--painter-model",
                    str(root / "repeating-patterns.pth"),
                    "--input",
                    str(root / "input"),
                    "--output",
                    str(root / "output"),
                ],
            ):
                args = runner.parse_args()
            self.assertTrue(args.auto_route)
            self.assertIsNone(args.model)
            self.assertEqual(args.painter_share, 30)
            self.assertEqual(args.architect_model.name, "solid-surfaces.safetensors")
            self.assertEqual(args.painter_model.name, "repeating-patterns.pth")

    def test_auto_route_selection_caps_painter_share_and_uses_highest_texture_scores(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            scored = [
                (root / f"texture-{index:02d}.png", {"score": score})
                for index, score in enumerate((0.90, 0.80, 0.70, 0.60, 0.50, 0.40, 0.20, 0.10, 0.05, 0.0))
            ]
            painter_files = runner._select_painter_files(scored, painter_share=30)
            self.assertEqual(
                painter_files,
                {root / "texture-00.png", root / "texture-01.png", root / "texture-02.png"},
            )

    def test_auto_route_never_rounds_the_painter_cap_up(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            for file_count, share in ((12, 30), (11, 90), (19, 10)):
                with self.subTest(file_count=file_count, share=share):
                    scored = [
                        (root / f"{file_count}-{index:02d}.png", {"score": 1.0})
                        for index in range(file_count)
                    ]
                    selected = runner._select_painter_files(scored, painter_share=share)
                    self.assertLessEqual(len(selected), file_count * share // 100)

    def test_auto_route_does_not_force_painter_when_no_texture_score_is_eligible(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            scored = [
                (root / f"flat-{index:02d}.png", {"score": 0.1})
                for index in range(10)
            ]
            self.assertEqual(runner._select_painter_files(scored, painter_share=30), set())

    def test_auto_route_applies_strict_cap_to_small_batches(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            scored = [
                (root / "smooth.png", {"score": 0.20}),
                (root / "grass-like.png", {"score": 0.80}),
            ]
            self.assertEqual(runner._select_painter_files(scored, painter_share=30), set())
            self.assertEqual(
                runner._select_painter_files(scored, painter_share=50),
                {root / "grass-like.png"},
            )


if __name__ == "__main__":
    unittest.main()
