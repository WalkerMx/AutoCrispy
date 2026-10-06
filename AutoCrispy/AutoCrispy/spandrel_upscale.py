"""Folder-based Spandrel inference bridge for AutoCrispy.

Loads a PLKSR/DAT2 checkpoint or a Spandrel-recognized generic checkpoint,
then processes supported files in one input directory. The selected model is
kept resident for the whole batch.
"""

from __future__ import annotations

import argparse
import base64
import gc
import sys
import traceback
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
    parser.add_argument("--input", type=Path, help="Input folder")
    parser.add_argument("--output", type=Path, help="Output folder")
    parser.add_argument(
        "--tile-size",
        type=int,
        default=512,
        help="Maximum input tile edge in pixels (default: 512; 0 tries full-image inference)",
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
    if args.list_models is None and (args.model is None or args.input is None or args.output is None):
        parser.error("model, --input, and --output are required unless --list-models is used")
    return args


def _load_model(
    model_path: Path, force_cpu: bool, generic_model: bool = False
) -> tuple[Any, Any, Any, str]:
    try:
        import torch
        from spandrel import ImageModelDescriptor, ModelLoader
    except ImportError as error:
        raise RuntimeError(
            "Spandrel upscalers need PyTorch and Spandrel. Follow "
            "PLKSR_SETUP.md to install them in AutoCrispy's Python environment."
        ) from error

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

    # Use the checkpoint's advertised precision support. DAT does not support
    # fp16 in Spandrel, but supported GPUs can run it in bfloat16 instead.
    if device.type == "cuda" and descriptor.supports_half:
        dtype = torch.float16
    elif (
        device.type == "cuda"
        and getattr(descriptor, "supports_bfloat16", False)
        and hasattr(torch.cuda, "is_bf16_supported")
        and torch.cuda.is_bf16_supported()
    ):
        dtype = torch.bfloat16
    else:
        dtype = torch.float32

    descriptor.to(device, dtype)
    descriptor.model.eval()
    return descriptor, device, dtype, str(device)


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


def _process_image(
    input_path: Path,
    output_path: Path,
    model: Any,
    device: Any,
    dtype: Any,
    tile_size: int,
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
    if args.model is None or args.input is None or args.output is None:
        raise ValueError("A checkpoint, input folder, and output folder are required.")
    if not args.input.is_dir():
        raise NotADirectoryError(f"Input folder not found: {args.input}")
    args.output.mkdir(parents=True, exist_ok=True)

    model, device, dtype, device_name = _load_model(
        args.model, args.cpu, args.generic_model
    )
    profile = MODEL_PROFILES.get(args.model.name.casefold())
    display_name = profile[0] if profile is not None else str(model.architecture.id)
    print(
        f"Loaded {args.model.name}: {display_name} x{model.scale} on "
        f"{device_name} ({dtype})."
    )

    files = sorted(
        path
        for path in args.input.iterdir()
        if path.is_file() and path.suffix.casefold() in SUPPORTED_EXTENSIONS
    )
    for input_path in files:
        _process_image(
            input_path,
            args.output / input_path.name,
            model,
            device,
            dtype,
            args.tile_size,
        )

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
