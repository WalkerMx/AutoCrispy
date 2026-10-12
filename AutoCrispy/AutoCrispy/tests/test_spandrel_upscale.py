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
            output_folder = root / "output"
            input_folder.mkdir()
            architect_file = input_folder / "a-architect.png"
            painter_file = input_folder / "z-painter.png"
            unsupported_file = input_folder / "ignored.dds"
            architect_file.touch()
            painter_file.touch()
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
                painter_share=30,
                debug=False,
            )

            def fake_load_model(path: Path, _force_cpu: bool, generic_model: bool):
                model = architect if path == architect_path else painter
                return model, FakeDevice("cuda:0"), "float32", "cuda:0"

            def fake_features(path: Path) -> dict[str, float]:
                return {"score": 0.5 if path == painter_file else 0.1}

            processed_files: list[Path] = []

            def fake_process_image(input_path: Path, *_args: object) -> None:
                processed_files.append(input_path)

            output = io.StringIO()
            with (
                patch.object(runner, "_load_model", side_effect=fake_load_model),
                patch.object(runner, "_texture_features", side_effect=fake_features),
                patch.object(runner, "_process_image", side_effect=fake_process_image),
                contextlib.redirect_stdout(output),
            ):
                result = runner._run_auto_route(args)

            self.assertEqual(result, 0)
            self.assertCountEqual(processed_files, [architect_file, painter_file])
            self.assertNotIn(unsupported_file, processed_files)
            log = output.getvalue()
            self.assertIn("AUTOCRISPY_ROUTE_SUMMARY: total=2; Architect=1; Painter=1", log)
            self.assertIn(f"AUTOCRISPY_MODELS: Architect={architect_path.resolve()}", log)
            self.assertIn(f"Painter={painter_path.resolve()}", log)
            self.assertIn("AUTOCRISPY_RESULT: 1/2 · Architect · a-architect.png · OK", log)
            self.assertIn("AUTOCRISPY_RESULT: 2/2 · Painter · z-painter.png · OK", log)

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
            self.assertEqual(args.model, model_path)
            self.assertIsNone(args.architect_model)
            self.assertIsNone(args.painter_model)

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
            self.assertIn("AUTOCRISPY_ROUTE_PREVIEW_SUMMARY: total=2; Architect=1; Painter=1", log)
            self.assertIn("AUTOCRISPY_ROUTE_PREVIEW_ITEM:", log)
            self.assertIn("\tPainter\t0.720000\t", log)
            self.assertIn("\tArchitect\t0.120000\t", log)
            self.assertIn("Eligible and selected for Painter", log)
            self.assertIn("Below minimum Painter score", log)
            self.assertIn("Sample=all 2 (sorted by path)", log)

    def test_preview_limit_scores_only_first_sorted_supported_images(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            input_folder = Path(temporary) / "input"
            input_folder.mkdir()
            first = input_folder / "a.png"
            second = input_folder / "b.png"
            third = input_folder / "c.png"
            unsupported = input_folder / "ignored.dds"
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
            self.assertEqual(analyzed, [first, second])
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

    def test_auto_route_does_not_force_painter_when_no_texture_score_is_eligible(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            scored = [
                (root / f"flat-{index:02d}.png", {"score": 0.1})
                for index in range(10)
            ]
            self.assertEqual(runner._select_painter_files(scored, painter_share=30), set())

    def test_auto_route_uses_absolute_texture_threshold_for_small_batches(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            scored = [
                (root / "smooth.png", {"score": 0.20}),
                (root / "grass-like.png", {"score": 0.80}),
            ]
            self.assertEqual(
                runner._select_painter_files(scored, painter_share=30),
                {root / "grass-like.png"},
            )


if __name__ == "__main__":
    unittest.main()
