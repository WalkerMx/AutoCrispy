"""Folder-based Spandrel inference bridge for AutoCrispy.

Loads a PLKSR/DAT2 checkpoint or a Spandrel-recognized generic checkpoint,
then processes supported files in one input directory. An optional experimental
feature router can dispatch images between Architect and Painter checkpoints.
The selected model or model pair remains resident for the batch.
"""

from __future__ import annotations

import argparse
import base64
import gc
import math
import os
import sys
import traceback
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from typing import Any


SUPPORTED_EXTENSIONS = {
    ".png",
    ".jpg",
    ".jpeg",
    ".bmp",
    ".tif",
    ".tiff",
    ".webp",
    ".tga",
}
PLKSR_MODEL_NAME = "4x-PBRify_RPLKSRd_V3.pth"
DAT2_MODEL_NAME = "4x-PBRify_UpscalerV4.pth"
MODEL_PROFILES = {
    PLKSR_MODEL_NAME.casefold(): ("RealPLKSR-DySample", None),
    DAT2_MODEL_NAME.casefold(): ("DAT2", "DAT"),
}
MODEL_SCALE = 4
SUPPORTED_GENERIC_SCALES = {1, MODEL_SCALE}
TILE_OVERLAP = 32
MODEL_FILE_EXTENSIONS = {".pth", ".pt", ".ckpt", ".safetensors"}
AUTO_ROUTE_MIN_PAINTER_SCORE = 0.34
DEFAULT_CPU_THREAD_LIMIT = 16
_CONFIGURED_TORCH_MODULE: Any = None


def _has_supported_generic_purpose(descriptor: Any) -> bool:
    # Spandrel labels 1x ESRGAN/restoration checkpoints as "Restoration".
    return descriptor.purpose == "SR" or (
        descriptor.scale == 1 and descriptor.purpose == "Restoration"
    )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Process a folder with 1x RGB restoration or 4x RGB super-resolution models."
    )
    parser.add_argument(
        "--list-models",
        type=Path,
        help="List 1x RGB restoration or 4x RGB super-resolution checkpoints and exit",
    )
    parser.add_argument(
        "model", type=Path, nargs="?", help="PyTorch checkpoint to run"
    )
    parser.add_argument(
        "--auto-route",
        action="store_true",
        help="Route each texture to an Architect or Painter model using image features",
    )
    parser.add_argument(
        "--preview-route",
        action="store_true",
        help="Analyze input textures and report route assignments without upscaling",
    )
    parser.add_argument(
        "--preview-limit",
        type=int,
        default=0,
        help="Maximum number of sorted input images to analyze for a preview (0 means all)",
    )
    parser.add_argument("--architect-model", type=Path, help="4x Architect checkpoint for auto routing")
    parser.add_argument("--painter-model", type=Path, help="4x Painter checkpoint for auto routing")
    parser.add_argument(
        "--painter-share",
        type=int,
        default=30,
        help="Strict maximum Painter share for every batch; floored to whole images (default: 30 percent)",
    )
    parser.add_argument(
        "--painter-threshold",
        type=float,
        default=AUTO_ROUTE_MIN_PAINTER_SCORE,
        help="Minimum feature score for Painter eligibility (default: 0.34)",
    )
    parser.add_argument("--input", type=Path, help="Input folder")
    parser.add_argument("--output", type=Path, help="Output folder")
    parser.add_argument(
        "--tile-size",
        type=int,
        default=1024,
        help="Maximum input tile edge in pixels (default: 1024; 0 tries full-image inference)",
    )
    parser.add_argument(
        "--cpu-threads",
        type=int,
        default=0,
        help="CPU worker threads for analysis/inference (0 uses up to 16; default: auto)",
    )
    parser.add_argument(
        "--precision",
        choices=("auto", "fp16", "fp32"),
        default="auto",
        help="Auto uses supported FP16 on CUDA; explicit FP16 requires checkpoint support; FP32 disables TF32",
    )
    parser.add_argument(
        "--normal-map-mode",
        choices=("none", "normalize-xyz", "rebuild-z"),
        default="none",
        help=(
            "Optional post-processing for tangent-space normals: normalize XYZ, "
            "or rebuild positive Z from R/G (BC5/RG normals)"
        ),
    )
    parser.add_argument(
        "--cpu", action="store_true", help="Force CPU inference instead of CUDA"
    )
    parser.add_argument(
        "--generic-model",
        action="store_true",
        help="Allow any Spandrel-recognized 1x RGB restoration or 4x RGB super-resolution checkpoint",
    )
    parser.add_argument(
        "--debug", action="store_true", help="Print a full traceback on errors"
    )
    args = parser.parse_args()
    if args.cpu_threads < 0:
        parser.error("--cpu-threads must be 0 (automatic) or a positive number")
    if args.preview_route:
        if args.list_models is not None:
            parser.error("Do not combine --preview-route with --list-models")
        if args.input is None:
            parser.error("--preview-route requires --input")
        if args.model is not None or args.auto_route:
            parser.error("--preview-route does not run a model; omit the model and --auto-route")
        if not 0 <= args.painter_share <= 100:
            parser.error("--painter-share must be between 0 and 100")
        if not 0 <= args.painter_threshold <= 1:
            parser.error("--painter-threshold must be between 0 and 1")
        if args.preview_limit < 0:
            parser.error("--preview-limit must be 0 or greater")
    elif args.list_models is None:
        if args.input is None or args.output is None:
            parser.error("--input and --output are required unless --list-models or --preview-route is used")
        if args.auto_route:
            if args.model is not None:
                parser.error("Do not pass a single model together with --auto-route")
            if args.architect_model is None or args.painter_model is None:
                parser.error("--auto-route requires both --architect-model and --painter-model")
            if not 0 <= args.painter_share <= 100:
                parser.error("--painter-share must be between 0 and 100")
            if not 0 <= args.painter_threshold <= 1:
                parser.error("--painter-threshold must be between 0 and 1")
        elif args.model is None:
            parser.error("A checkpoint is required unless --auto-route or --list-models is used")
    return args


def _resolve_cpu_threads(requested: int = 0) -> int:
    if requested < 0:
        raise ValueError("CPU thread count must be 0 (automatic) or a positive number.")
    available = max(1, os.cpu_count() or 1)
    return min(requested if requested > 0 else DEFAULT_CPU_THREAD_LIMIT, available)


def _configure_torch_runtime(
    torch: Any, device: Any, cpu_threads: int = 0, precision: str = "auto"
) -> int:
    """Set conservative CPU parallelism and enable safe Ada/CUDA fast paths."""
    global _CONFIGURED_TORCH_MODULE
    thread_count = _resolve_cpu_threads(cpu_threads)
    if _CONFIGURED_TORCH_MODULE is not torch:
        set_num_threads = getattr(torch, "set_num_threads", None)
        if callable(set_num_threads):
            set_num_threads(thread_count)
        set_interop_threads = getattr(torch, "set_num_interop_threads", None)
        if callable(set_interop_threads):
            try:
                # Inference is sequential; one inter-op worker avoids over-subscribing
                # the 16 intra-op CPU workers on high-core-count desktop CPUs.
                set_interop_threads(1)
            except RuntimeError:
                # PyTorch only permits this setting before parallel work has started.
                pass

    if getattr(device, "type", "") == "cuda":
        backends = getattr(torch, "backends", None)
        cudnn = getattr(backends, "cudnn", None)
        if cudnn is not None:
            cudnn.benchmark = True
            if hasattr(cudnn, "allow_tf32"):
                cudnn.allow_tf32 = precision != "fp32"
        cuda_backend = getattr(backends, "cuda", None)
        matmul = getattr(cuda_backend, "matmul", None)
        if matmul is not None and hasattr(matmul, "allow_tf32"):
            matmul.allow_tf32 = precision != "fp32"

    _CONFIGURED_TORCH_MODULE = torch
    return thread_count


def _load_model(
    model_path: Path,
    force_cpu: bool,
    generic_model: bool = False,
    cpu_threads: int = 0,
    precision: str = "auto",
) -> tuple[Any, Any, Any, str]:
    try:
        import torch
        from spandrel import ImageModelDescriptor, ModelLoader
    except ImportError as error:
        raise RuntimeError(
            "Spandrel upscalers need PyTorch and Spandrel. Follow "
            "PLKSR_SETUP.md to install them in AutoCrispy's Python environment."
        ) from error

    precision = (precision or "auto").casefold()
    if precision not in {"auto", "fp16", "fp32"}:
        raise ValueError("Precision must be auto, fp16, or fp32.")

    profile = MODEL_PROFILES.get(model_path.name.casefold())
    if not generic_model and profile is None:
        supported = f"{PLKSR_MODEL_NAME}, {DAT2_MODEL_NAME}, or pass --generic-model"
        raise ValueError(f"This AutoCrispy runner supports only: {supported}.")
    if not model_path.is_file():
        raise FileNotFoundError(f"Checkpoint not found: {model_path}")

    display_name, expected_architecture = profile if profile is not None else ("Spandrel", None)
    device = torch.device(
        "cpu" if force_cpu or not torch.cuda.is_available() else "cuda:0"
    )
    _configure_torch_runtime(torch, device, cpu_threads, precision)
    if precision == "fp16" and device.type != "cuda":
        raise ValueError("FP16 precision requires CUDA; use auto or fp32 for CPU inference.")
    descriptor = ModelLoader(device).load_from_file(model_path)
    if not isinstance(descriptor, ImageModelDescriptor):
        raise ValueError("The selected checkpoint is not an image super-resolution model.")

    architecture = str(descriptor.architecture.id)
    tags = {str(tag).casefold() for tag in descriptor.tags}
    allowed_scales = SUPPORTED_GENERIC_SCALES if generic_model else {MODEL_SCALE}
    purpose_valid = (
        _has_supported_generic_purpose(descriptor)
        if generic_model
        else descriptor.purpose == "SR"
    )
    common_valid = (
        purpose_valid
        and descriptor.scale in allowed_scales
        and descriptor.input_channels == 3
        and descriptor.output_channels == 3
    )
    if generic_model:
        model_valid = True
    elif display_name == "RealPLKSR-DySample":
        model_valid = "real" in tags and "dysample" in tags
    else:
        model_valid = (
            expected_architecture is not None
            and architecture.casefold() == expected_architecture.casefold()
        )

    if not common_valid or not model_valid:
        if generic_model:
            expected_description = "a Spandrel-supported 1x RGB restoration or 4x RGB super-resolution"
        elif display_name == "RealPLKSR-DySample":
            expected_description = "a 4x RealPLKSR-DySample RGB"
        else:
            expected_description = "a 4x DAT RGB"
        raise ValueError(
            f"Expected {expected_description} checkpoint {model_path.name}; "
            f"Spandrel detected architecture={architecture}, purpose={descriptor.purpose}, "
            f"scale={descriptor.scale}, "
            f"channels={descriptor.input_channels}->{descriptor.output_channels}, "
            f"tags={sorted(tags)}."
        )

    # Use fp16 only when the checkpoint explicitly advertises support. Some
    # checkpoints advertise bfloat16 but still mix float32-only operations in
    # their forward pass.
    if precision == "fp16":
        if not descriptor.supports_half:
            raise ValueError(
                f"Checkpoint {model_path.name} does not advertise FP16 support; use auto or fp32."
            )
        dtype = torch.float16
    elif precision == "fp32":
        dtype = torch.float32
    elif device.type == "cuda" and descriptor.supports_half:
        dtype = torch.float16
    else:
        dtype = torch.float32

    descriptor.to(device, dtype)
    descriptor.model.eval()
    device_name = str(device)
    if device.type == "cuda":
        get_device_name = getattr(torch.cuda, "get_device_name", None)
        if callable(get_device_name):
            try:
                gpu_name = get_device_name(device)
                if gpu_name:
                    device_name = f"{device_name} ({gpu_name})"
            except (RuntimeError, TypeError):
                pass
    return descriptor, device, dtype, device_name


def _infer_tile(model: Any, image: Any, device: Any, dtype: Any) -> Any:
    import numpy as np
    import torch

    tensor = torch.from_numpy(np.ascontiguousarray(image.transpose(2, 0, 1)))
    tensor = tensor.unsqueeze(0).to(device=device, dtype=dtype)
    with torch.inference_mode():
        result = model(tensor)
    return result.squeeze(0).permute(1, 2, 0).float().cpu().numpy()


def _tile_starts(length: int, core_size: int) -> list[int]:
    if length <= core_size:
        return [0]
    return list(range(0, length, core_size))


def _upscale_rgb(
    image: Any, model: Any, device: Any, dtype: Any, tile_size: int
) -> Any:
    import numpy as np

    height, width, _ = image.shape
    scale = int(model.scale)
    if tile_size <= 0 or (height <= tile_size and width <= tile_size):
        return _infer_tile(model, image, device, dtype)

    overlap = min(TILE_OVERLAP, max(8, tile_size // 8))
    core_size = max(1, tile_size - 2 * overlap)
    y_starts = _tile_starts(height, core_size)
    x_starts = _tile_starts(width, core_size)
    output = np.empty((height * scale, width * scale, 3), dtype=np.float32)

    for core_y0 in y_starts:
        core_y1 = min(core_y0 + core_size, height)
        input_y0 = max(0, core_y0 - overlap)
        input_y1 = min(height, core_y1 + overlap)
        for core_x0 in x_starts:
            core_x1 = min(core_x0 + core_size, width)
            input_x0 = max(0, core_x0 - overlap)
            input_x1 = min(width, core_x1 + overlap)

            tile_output = _infer_tile(
                model,
                image[input_y0:input_y1, input_x0:input_x1, :],
                device,
                dtype,
            )
            out_y0 = (core_y0 - input_y0) * scale
            out_x0 = (core_x0 - input_x0) * scale
            out_y1 = out_y0 + (core_y1 - core_y0) * scale
            out_x1 = out_x0 + (core_x1 - core_x0) * scale
            output[
                core_y0 * scale : core_y1 * scale,
                core_x0 * scale : core_x1 * scale,
                :,
            ] = tile_output[out_y0:out_y1, out_x0:out_x1, :]

    return output


def _get_model_tiling_mode(model: Any) -> str | None:
    """Read Spandrel's tiling recommendation without requiring its enum import."""
    tiling = getattr(model, "tiling", None)
    if tiling is None:
        return None
    mode = getattr(tiling, "name", None)
    if mode is None:
        mode = str(tiling).rsplit(".", 1)[-1]
    normalized = str(mode).strip().casefold()
    return normalized or None


def _report_model_tiling_metadata(model: Any, model_label: str) -> None:
    mode = _get_model_tiling_mode(model)
    if mode == "internal":
        print(
            f"{model_label} declares internal tiling; AutoCrispy will bypass "
            "external tiling and OOM tile retries.",
            flush=True,
        )
    elif mode == "discouraged":
        print(
            f"{model_label} discourages external tiling because it may cause "
            "artifacts; AutoCrispy keeps its configured tile size and OOM fallback.",
            flush=True,
        )


def _is_out_of_memory(error: RuntimeError) -> bool:
    return "out of memory" in str(error).casefold() or "cuda error: out of memory" in str(
        error
    ).casefold()


def _upscale_with_fallback(
    image: Any, model: Any, device: Any, dtype: Any, tile_size: int
) -> Any:
    import torch

    if tile_size < 0:
        raise ValueError("Tile size must be zero (untiled) or a positive number of pixels.")

    if _get_model_tiling_mode(model) == "internal":
        # External tiles can interfere with models that split their own inputs.
        # Do not retry with external tiles after OOM; trust Spandrel's metadata.
        return _infer_tile(model, image, device, dtype)

    # Zero means try the full image first. If that does not fit in memory,
    # switch to tiled inference and keep reducing the tile size on OOM.
    current_tile_size = 0 if tile_size == 0 else max(64, tile_size)
    while True:
        try:
            return _upscale_rgb(image, model, device, dtype, current_tile_size)
        except RuntimeError as error:
            if not _is_out_of_memory(error):
                raise
            if current_tile_size == 0:
                current_tile_size = 512
            elif current_tile_size <= 64:
                raise
            else:
                current_tile_size = max(64, current_tile_size // 2)
            if device.type == "cuda":
                torch.cuda.empty_cache()
            print(
                f"Not enough memory; retrying with {current_tile_size}px tiles.",
                file=sys.stderr,
            )


def _postprocess_normal_map(rgb: Any, mode: str) -> Any:
    """Repair vector lengths after RGB upscaling when the output is a normal map."""
    mode = str(mode).strip().casefold()
    if mode == "none":
        return rgb
    if mode not in {"normalize-xyz", "rebuild-z"}:
        raise ValueError(f"Unsupported normal-map mode: {mode}")

    import numpy as np

    encoded = np.asarray(rgb, dtype=np.float32)
    if encoded.ndim != 3 or encoded.shape[2] != 3:
        raise ValueError("Normal-map post-processing expects an H×W×3 RGB image.")

    normals = encoded * 2.0 - 1.0
    if mode == "normalize-xyz":
        lengths = np.linalg.norm(normals, axis=2, keepdims=True)
        zero_length = lengths[:, :, 0] <= 1e-8
        normals = normals / np.maximum(lengths, 1e-8)
        if bool(np.any(zero_length)):
            normals[zero_length] = (0.0, 0.0, 1.0)
    else:
        xy = normals[:, :, :2].copy()
        xy_lengths = np.linalg.norm(xy, axis=2, keepdims=True)
        # Clamp invalid XY vectors to the unit circle before reconstructing Z.
        outside_unit_disk = xy_lengths[:, :, 0] >= 1.0
        xy = xy / np.maximum(xy_lengths, 1.0)
        z = np.sqrt(np.maximum(1.0 - np.sum(xy * xy, axis=2, keepdims=True), 0.0))
        z = np.where(outside_unit_disk[:, :, None], 0.0, z)
        normals = np.concatenate((xy, z), axis=2)

    return np.clip((normals + 1.0) * 0.5, 0.0, 1.0)


def _process_image(
    input_path: Path,
    output_path: Path,
    model: Any,
    device: Any,
    dtype: Any,
    tile_size: int,
    normal_map_mode: str = "none",
) -> None:
    import numpy as np
    from PIL import Image, ImageOps

    with Image.open(input_path) as source:
        source.load()
        source = ImageOps.exif_transpose(source)
        has_alpha = "A" in source.getbands() or "transparency" in source.info
        icc_profile = source.info.get("icc_profile")

        if has_alpha:
            rgba = np.asarray(source.convert("RGBA"), dtype=np.uint8)
            rgb = rgba[:, :, :3]
            alpha = Image.fromarray(rgba[:, :, 3])
        else:
            rgb = np.asarray(source.convert("RGB"), dtype=np.uint8)
            alpha = None

    rgb_float = rgb.astype(np.float32) / 255.0
    upscaled = _upscale_with_fallback(rgb_float, model, device, dtype, tile_size)
    upscaled = _postprocess_normal_map(upscaled, normal_map_mode)
    upscaled = np.clip(np.rint(upscaled * 255.0), 0, 255).astype(np.uint8)
    model_scale = int(model.scale)
    expected_size = (rgb.shape[1] * model_scale, rgb.shape[0] * model_scale)

    if upscaled.shape[:2] != (expected_size[1], expected_size[0]):
        raise ValueError(
            f"Unexpected output dimensions for {input_path.name}: "
            f"{upscaled.shape[1]}x{upscaled.shape[0]} instead of "
            f"{expected_size[0]}x{expected_size[1]}."
        )

    if alpha is not None:
        resampling = getattr(Image, "Resampling", Image).LANCZOS
        alpha = alpha.resize(expected_size, resampling)
        result = Image.fromarray(np.dstack((upscaled, np.asarray(alpha))))
    else:
        result = Image.fromarray(upscaled)

    save_options: dict[str, Any] = {}
    if icc_profile:
        save_options["icc_profile"] = icc_profile
    if input_path.suffix.casefold() in {".jpg", ".jpeg"}:
        result = result.convert("RGB")
        save_options.update(quality=95, subsampling=0)

    output_path.parent.mkdir(parents=True, exist_ok=True)
    result.save(output_path, **save_options)


def _pearson_correlation(left: Any, right: Any) -> float:
    import numpy as np

    left = np.asarray(left, dtype=np.float32).ravel()
    right = np.asarray(right, dtype=np.float32).ravel()
    if left.size < 4 or left.size != right.size:
        return 0.0
    left = left - float(left.mean())
    right = right - float(right.mean())
    denominator = float(np.sqrt(np.sum(left * left) * np.sum(right * right)))
    if denominator <= 1e-8:
        return 0.0
    return float(np.sum(left * right) / denominator)


def _texture_features(input_path: Path) -> dict[str, float]:
    """Estimate fine texture and repetition without a separate ML model.

    The feature score is a routing heuristic, not semantic recognition: it cannot
    reliably identify objects such as grass, skin, stone, or brick by name.
    """
    import numpy as np
    from PIL import Image, ImageFilter, ImageOps

    with Image.open(input_path) as source:
        source.load()
        gray_image = ImageOps.exif_transpose(source).convert("L")
        resampling = getattr(Image, "Resampling", Image).BILINEAR
        gray_image.thumbnail((96, 96), resampling)
        gray = np.asarray(gray_image, dtype=np.float32) / 255.0
        blurred = np.asarray(
            gray_image.filter(ImageFilter.GaussianBlur(radius=1.0)), dtype=np.float32
        ) / 255.0

    height, width = gray.shape
    if height < 3 or width < 3:
        return {
            "score": 0.0,
            "detail": 0.0,
            "edge_density": 0.0,
            "orientation_entropy": 0.0,
            "local_pattern_entropy": 0.0,
            "periodicity": 0.0,
        }

    high_pass = gray - blurred
    detail = min(1.0, float(np.mean(np.abs(high_pass))) / 0.12)

    # Local binary-pattern entropy estimates how varied the tiny neighborhoods
    # are. Uniformly repeating geometric cells tend to have fewer patterns than
    # natural, fine-grained surfaces, without needing a semantic classifier.
    pattern_codes = np.zeros((height - 2, width - 2), dtype=np.uint8)
    center = gray[1:-1, 1:-1]
    neighbors = ((-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1))
    for bit, (offset_y, offset_x) in enumerate(neighbors):
        neighbor = gray[1 + offset_y : height - 1 + offset_y, 1 + offset_x : width - 1 + offset_x]
        pattern_codes |= (neighbor >= center).astype(np.uint8) << bit
    pattern_histogram = np.bincount(pattern_codes.ravel(), minlength=256).astype(np.float32)
    pattern_probabilities = pattern_histogram / float(pattern_histogram.sum())
    pattern_probabilities = pattern_probabilities[pattern_probabilities > 0]
    local_pattern_entropy = float(
        -np.sum(pattern_probabilities * np.log(pattern_probabilities)) / np.log(256.0)
    )

    gradient_x = gray[1:, 1:] - gray[1:, :-1]
    gradient_y = gray[1:, 1:] - gray[:-1, 1:]
    magnitude = np.hypot(gradient_x, gradient_y)
    edge_mask = magnitude >= 0.045
    edge_density = float(np.mean(edge_mask))

    orientation_entropy = 0.0
    if bool(np.any(edge_mask)):
        angles = np.mod(np.arctan2(gradient_y[edge_mask], gradient_x[edge_mask]), np.pi)
        orientation_histogram, _ = np.histogram(angles, bins=8, range=(0.0, np.pi))
        histogram_sum = int(orientation_histogram.sum())
        if histogram_sum > 0:
            probabilities = orientation_histogram.astype(np.float32) / histogram_sum
            probabilities = probabilities[probabilities > 0]
            orientation_entropy = float(
                -np.sum(probabilities * np.log(probabilities)) / np.log(8.0)
            )

    periodicity = 0.0
    for shift in (2, 3, 4, 6, 8, 12, 16, 20, 24):
        if width > shift * 2:
            periodicity = max(
                periodicity,
                _pearson_correlation(high_pass[:, :-shift], high_pass[:, shift:]),
            )
        if height > shift * 2:
            periodicity = max(
                periodicity,
                _pearson_correlation(high_pass[:-shift, :], high_pass[shift:, :]),
            )
    periodicity = max(0.0, min(1.0, periodicity))

    detail_score = min(1.0, detail)
    edge_score = min(1.0, edge_density / 0.35)
    # Natural local-pattern diversity and mixed edge directions favor Painter.
    # Highly regular geometric contours still influence the score through edges
    # and periodicity, but repeatedness is only a small boost on diverse detail.
    painter_score = (
        0.08 * detail_score
        + 0.05 * edge_score
        + 0.15 * orientation_entropy
        + 0.60 * local_pattern_entropy
        + 0.12 * periodicity * local_pattern_entropy
    )
    return {
        "score": max(0.0, min(1.0, painter_score)),
        "detail": detail_score,
        "edge_density": edge_density,
        "orientation_entropy": orientation_entropy,
        "local_pattern_entropy": local_pattern_entropy,
        "periodicity": periodicity,
    }


def _path_is_within(path: Path, parent: Path) -> bool:
    try:
        path.relative_to(parent)
        return True
    except ValueError:
        return False


def _validate_input_output_paths(input_folder: Path, output_folder: Path) -> None:
    if input_folder.resolve() == output_folder.resolve():
        raise ValueError(
            "Input and output folders must be different to avoid overwriting source images."
        )


def _iter_supported_images(
    input_folder: Path, excluded_folder: Path | None = None
) -> list[Path]:
    """Find supported images in path order, pruning an output subtree if supplied."""
    input_root = input_folder.resolve()
    excluded_root = excluded_folder.resolve() if excluded_folder is not None else None
    if excluded_root == input_root:
        raise ValueError(
            "Input and output folders must be different to avoid overwriting source images."
        )
    exclude_output_subtree = (
        excluded_root is not None
        and _path_is_within(excluded_root, input_root)
    )

    images: list[Path] = []
    pending_folders = [input_folder]
    while pending_folders:
        folder = pending_folders.pop()
        try:
            entries = sorted(
                folder.iterdir(), key=lambda path: (path.name.casefold(), path.name)
            )
        except OSError:
            continue
        for path in entries:
            try:
                if path.is_dir():
                    if path.is_symlink() or (
                        excluded_root is not None and path.resolve() == excluded_root
                    ):
                        continue
                    pending_folders.append(path)
                    continue
                if not path.is_file() or path.suffix.casefold() not in SUPPORTED_EXTENSIONS:
                    continue
                if (
                    exclude_output_subtree
                    and excluded_root is not None
                    and _path_is_within(path.resolve(), excluded_root)
                ):
                    continue
                images.append(path)
            except OSError:
                # Files can disappear while the game is writing a dump.
                continue

    return sorted(
        images,
        key=lambda path: (
            path.relative_to(input_folder).as_posix().casefold(),
            path.relative_to(input_folder).as_posix(),
        ),
    )


def _select_painter_files(
    scored_files: list[tuple[Path, dict[str, float]]],
    painter_share: int,
    painter_threshold: float = AUTO_ROUTE_MIN_PAINTER_SCORE,
) -> set[Path]:
    """Select eligible high-texture images without exceeding the share cap."""
    if not 0 <= painter_share <= 100:
        raise ValueError("Painter share must be between 0 and 100 percent.")
    if not 0 <= painter_threshold <= 1:
        raise ValueError("Painter threshold must be between 0 and 1.")
    eligible = [
        (path, features)
        for path, features in scored_files
        if features["score"] >= painter_threshold
    ]
    if not eligible or painter_share == 0:
        return set()

    target_count = int(math.floor(len(scored_files) * painter_share / 100.0))
    ranked = sorted(
        eligible,
        key=lambda item: (
            -item[1]["score"],
            item[0].name.casefold(),
            str(item[0]).casefold(),
        ),
    )
    return {path for path, _ in ranked[:target_count]}


def _preview_auto_route(args: argparse.Namespace) -> int:
    if not args.input.is_dir():
        raise NotADirectoryError(f"Input folder not found: {args.input}")
    available_files = _iter_supported_images(args.input)
    preview_limit = getattr(args, "preview_limit", 0)
    if preview_limit < 0:
        raise ValueError("Preview sample size must be 0 or greater.")
    files = available_files[:preview_limit] if preview_limit else available_files
    scored_files: list[tuple[Path, dict[str, float]]] = []
    report_interval = max(1, len(files) // 100)
    if files:
        worker_count = min(
            _resolve_cpu_threads(getattr(args, "cpu_threads", 0)), len(files)
        )
        with ThreadPoolExecutor(max_workers=worker_count) as executor:
            feature_results = executor.map(_texture_features, files)
            for index, (input_path, features) in enumerate(
                zip(files, feature_results), start=1
            ):
                if index == 1 or index % report_interval == 0 or index == len(files):
                    print(
                        f"AUTOCRISPY_PROGRESS: {index}/{len(files)} · Analyzing texture · {input_path.name}",
                        flush=True,
                    )
                scored_files.append((input_path, features))

    painter_files = _select_painter_files(
        scored_files, args.painter_share, args.painter_threshold
    )
    architect_count = len(files) - len(painter_files)
    sample_description = (
        f"all {len(available_files)}"
        if preview_limit == 0
        else f"first {len(files)} of {len(available_files)}"
    )
    print(
        f"AUTOCRISPY_ROUTE_PREVIEW_SUMMARY: total={len(files)}; "
        f"Architect={architect_count}; Painter={len(painter_files)}; "
        f"PainterCap={args.painter_share}%; Threshold={args.painter_threshold:.3f}; "
        f"Sample={sample_description} (sorted by path).",
        flush=True,
    )
    for input_path, features in scored_files:
        role = "Painter" if input_path in painter_files else "Architect"
        if features["score"] < args.painter_threshold:
            reason = "Below minimum Painter score"
        elif role == "Painter":
            reason = "Eligible and selected for Painter"
        else:
            reason = "Eligible, but outside the Painter share cap"
        encoded_path = base64.b64encode(
            str(input_path.resolve()).encode("utf-8")
        ).decode("ascii")
        print(
            "AUTOCRISPY_ROUTE_PREVIEW_ITEM:"
            f"{encoded_path}\t{role}\t{features['score']:.6f}\t"
            f"{features['detail']:.6f}\t{features['edge_density']:.6f}\t"
            f"{features['orientation_entropy']:.6f}\t"
            f"{features['local_pattern_entropy']:.6f}\t"
            f"{features['periodicity']:.6f}\t{reason}",
            flush=True,
        )
    return 0


def _report_texture_completed(input_path: Path) -> None:
    encoded_name = base64.b64encode(input_path.name.encode("utf-8")).decode("ascii")
    print(f"AUTOCRISPY_TEXTURE_COMPLETED: {encoded_name}", flush=True)


def _run_auto_route(args: argparse.Namespace) -> int:
    if not args.input.is_dir():
        raise NotADirectoryError(f"Input folder not found: {args.input}")
    _validate_input_output_paths(args.input, args.output)
    args.output.mkdir(parents=True, exist_ok=True)
    files = _iter_supported_images(args.input, args.output)
    if not files:
        print("Auto routing found no supported images.")
        return 0
    if args.architect_model.resolve() == args.painter_model.resolve():
        raise ValueError("Architect and Painter must be different checkpoints.")

    cpu_threads = getattr(args, "cpu_threads", 0)
    precision = getattr(args, "precision", "auto")
    architect, architect_device, architect_dtype, architect_device_name = _load_model(
        args.architect_model,
        args.cpu,
        generic_model=True,
        cpu_threads=cpu_threads,
        precision=precision,
    )
    painter, painter_device, painter_dtype, painter_device_name = _load_model(
        args.painter_model,
        args.cpu,
        generic_model=True,
        cpu_threads=cpu_threads,
        precision=precision,
    )
    if architect.scale != MODEL_SCALE or painter.scale != MODEL_SCALE:
        raise ValueError("Automatic Architect/Painter routing requires two 4x RGB models.")
    if architect.scale != painter.scale:
        raise ValueError("Architect and Painter checkpoints must use the same scale.")

    scored_files: list[tuple[Path, dict[str, float]]] = []
    report_interval = max(1, len(files) // 100)
    worker_count = min(_resolve_cpu_threads(cpu_threads), len(files))
    with ThreadPoolExecutor(max_workers=worker_count) as executor:
        feature_results = executor.map(_texture_features, files)
        for index, (input_path, features) in enumerate(
            zip(files, feature_results), start=1
        ):
            if index == 1 or index % report_interval == 0 or index == len(files):
                print(
                    f"AUTOCRISPY_PROGRESS: {index}/{len(files)} · Analyzing texture · {input_path.name}",
                    flush=True,
                )
            scored_files.append((input_path, features))
    painter_threshold = getattr(
        args, "painter_threshold", AUTO_ROUTE_MIN_PAINTER_SCORE
    )
    painter_files = _select_painter_files(
        scored_files, args.painter_share, painter_threshold
    )
    architect_count = len(files) - len(painter_files)
    print(
        f"AUTOCRISPY_ROUTE_SUMMARY: total={len(files)}; Architect={architect_count}; "
        f"Painter={len(painter_files)}; PainterCap={args.painter_share}%; "
        f"Threshold={painter_threshold:.3f} (feature-based, not semantic).",
        flush=True,
    )
    print(
        "AUTOCRISPY_MODELS: "
        f"Architect={args.architect_model.resolve()} "
        f"(device={architect_device_name}, dtype={architect_dtype}, "
        f"tiling={_get_model_tiling_mode(architect) or 'unknown'}); "
        f"Painter={args.painter_model.resolve()} "
        f"(device={painter_device_name}, dtype={painter_dtype}, "
        f"tiling={_get_model_tiling_mode(painter) or 'unknown'}).",
        flush=True,
    )
    _report_model_tiling_metadata(architect, "Architect model")
    _report_model_tiling_metadata(painter, "Painter model")

    routed_files = sorted(
        scored_files,
        key=lambda item: (item[0] in painter_files, item[0].name.casefold()),
    )
    for index, (input_path, features) in enumerate(routed_files, start=1):
        use_painter = input_path in painter_files
        model, device, dtype, role = (
            (painter, painter_device, painter_dtype, "Painter")
            if use_painter
            else (architect, architect_device, architect_dtype, "Architect")
        )
        print(
            f"AUTOCRISPY_PROGRESS: {index}/{len(files)} · {role} · {input_path.name}",
            flush=True,
        )
        if args.debug:
            print(
                f"Route detail: {input_path.name} -> {role}; "
                f"score={features['score']:.3f}, detail={features['detail']:.3f}, "
                f"edges={features['edge_density']:.3f}, "
                f"directions={features['orientation_entropy']:.3f}, "
                f"local-patterns={features['local_pattern_entropy']:.3f}, "
                f"repetition={features['periodicity']:.3f}",
                flush=True,
            )
        _process_image(
            input_path,
            args.output / input_path.relative_to(args.input),
            model,
            device,
            dtype,
            args.tile_size,
            getattr(args, "normal_map_mode", "none"),
        )
        print(
            f"AUTOCRISPY_RESULT: {index}/{len(files)} · {role} · {input_path.name} · OK",
            flush=True,
        )
        _report_texture_completed(input_path)

    print(f"Auto-routed and upscaled {len(files)} image(s).", flush=True)
    return 0


def list_supported_models(model_root: Path, debug: bool = False) -> int:
    if not model_root.is_dir():
        raise NotADirectoryError(f"Model folder not found: {model_root}")

    try:
        import torch
        from spandrel import ImageModelDescriptor, ModelLoader
    except ImportError as error:
        raise RuntimeError(
            "Listing Spandrel models needs PyTorch and Spandrel in AutoCrispy's Python environment."
        ) from error

    loader = ModelLoader(torch.device("cpu"))
    candidates: list[Path] = []
    pending_folders = [(model_root, 0)]
    while pending_folders:
        folder, folder_depth = pending_folders.pop()
        try:
            entries = sorted(folder.iterdir(), key=lambda path: path.name.casefold())
        except OSError:
            continue
        for path in entries:
            if path.is_dir() and not path.is_symlink():
                if folder_depth < 3:
                    pending_folders.append((path, folder_depth + 1))
                continue
            if (
                path.is_file()
                and path.suffix.casefold() in MODEL_FILE_EXTENSIONS
            ):
                candidates.append(path)
    candidates.sort(key=lambda path: str(path).casefold())

    for checkpoint in candidates:
        try:
            descriptor = loader.load_from_file(checkpoint)
            supported = (
                isinstance(descriptor, ImageModelDescriptor)
                and _has_supported_generic_purpose(descriptor)
                and descriptor.scale in SUPPORTED_GENERIC_SCALES
                and descriptor.input_channels == 3
                and descriptor.output_channels == 3
            )
            if supported:
                encoded_path = base64.b64encode(
                    str(checkpoint.resolve()).encode("utf-8")
                ).decode("ascii")
                encoded_architecture = base64.b64encode(
                    str(descriptor.architecture.id).encode("utf-8")
                ).decode("ascii")
                encoded_purpose = base64.b64encode(
                    str(descriptor.purpose).encode("utf-8")
                ).decode("ascii")
                metadata = "\t".join(
                    (
                        encoded_architecture,
                        str(descriptor.scale),
                        encoded_purpose,
                        str(descriptor.input_channels),
                        str(descriptor.output_channels),
                        (_get_model_tiling_mode(descriptor) or "unknown").upper(),
                        str(bool(getattr(descriptor, "supports_half", False))).lower(),
                    )
                )
                print(f"MODEL:{encoded_path}\t{metadata}", flush=True)
        except Exception as error:
            if debug:
                print(f"Skipping {checkpoint.name}: {error}", file=sys.stderr)
        finally:
            if "descriptor" in locals():
                del descriptor
            gc.collect()

    return 0


def run(args: argparse.Namespace) -> int:
    if args.preview_route:
        return _preview_auto_route(args)
    if args.auto_route:
        return _run_auto_route(args)
    if args.model is None or args.input is None or args.output is None:
        raise ValueError("A checkpoint, input folder, and output folder are required.")
    if not args.input.is_dir():
        raise NotADirectoryError(f"Input folder not found: {args.input}")
    _validate_input_output_paths(args.input, args.output)
    args.output.mkdir(parents=True, exist_ok=True)
    files = _iter_supported_images(args.input, args.output)
    if not files:
        print("No supported images found.")
        return 0

    model, device, dtype, device_name = _load_model(
        args.model,
        args.cpu,
        args.generic_model,
        getattr(args, "cpu_threads", 0),
        getattr(args, "precision", "auto"),
    )
    profile = MODEL_PROFILES.get(args.model.name.casefold())
    display_name = profile[0] if profile is not None else str(model.architecture.id)
    print(
        f"Loaded {args.model.name}: {display_name} x{model.scale} on "
        f"{device_name} ({dtype}; tiling={_get_model_tiling_mode(model) or 'unknown'})."
    )
    _report_model_tiling_metadata(model, f"Model {args.model.name}")

    for index, input_path in enumerate(files, start=1):
        relative_path = input_path.relative_to(args.input)
        print(
            f"AUTOCRISPY_PROGRESS: {index}/{len(files)} · Upscaling · {input_path.name}",
            flush=True,
        )
        try:
            _process_image(
                input_path,
                args.output / relative_path,
                model,
                device,
                dtype,
                args.tile_size,
                getattr(args, "normal_map_mode", "none"),
            )
        except Exception as error:
            raise RuntimeError(
                f"Failed to upscale {relative_path.as_posix()}: {error}"
            ) from error
        print(
            f"AUTOCRISPY_RESULT: {index}/{len(files)} · Upscaling · {input_path.name} · OK",
            flush=True,
        )
        _report_texture_completed(input_path)

    print(f"Upscaled {len(files)} image(s).")
    return 0


def main() -> int:
    args = parse_args()
    try:
        if args.list_models is not None:
            return list_supported_models(args.list_models, args.debug)
        return run(args)
    except Exception as error:
        print(f"AutoCrispy Spandrel error: {error}", file=sys.stderr)
        if args.debug:
            traceback.print_exc()
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
