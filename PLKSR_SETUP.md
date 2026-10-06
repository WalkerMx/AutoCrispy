# Spandrel models: PLKSR, DAT2, and generic checkpoints

AutoCrispy provides one Spandrel backend with a selectable model. PLKSR V3 and DAT2 V4 are model choices in that selector, not separate backends. These are optional texture processors, not the full PBRify material workflow: AutoCrispy does not generate normal, roughness, or AO maps, and it does not need the chaiNNer GUI. Checkpoint weights and the Python/PyTorch runtime are installed separately; do not add model weights to the Git repository.

- **PLKSR V3** — `4x-PBRify_RPLKSRd_V3.pth` (RealPLKSR-DySample; auto-selected when present)
- **DAT2 V4** — `4x-PBRify_UpscalerV4.pth` (DAT architecture / DAT2 model)
- **Generic Spandrel** — recognized 1× RGB restoration or 4× RGB super-resolution checkpoints, all shown in the same selector

## 1. Install checkpoints

Use one shared **`models`** folder for the checkpoints, either under the configured backend folder or beside AutoCrispy. PLKSR and DAT2 checkpoints are recognized by Spandrel and appear as model choices in the same selector. Example:

```text
Backend/
  models/
    4x-PBRify_RPLKSRd_V3.pth
    4x-PBRify_UpscalerV4.pth
    gameai-2.0.pth
    another-model.safetensors
```

Download the fixed checkpoints from their releases:

- [4x-PBRify_RPLKSRd_V3](https://github.com/Kim2091/Kim2091-Models/releases/tag/4x-PBRify_RPLKSRd_V3)
- [4x-PBRify_UpscalerV4](https://github.com/Kim2091/Kim2091-Models/releases/tag/4x-PBRify_UpscalerV4)

The optional [PLKSR source ZIP](https://github.com/dslisleedh/PLKSR) is not executed by AutoCrispy. Use the `.pth` asset from the V4 release. The PLKSR and DAT2 checkpoints are independent; install either or both.

The **Spandrel** selector uses the same `models` folder. It checks `.pth`, `.pt`, `.ckpt`, and `.safetensors` files there and up to three nested folders. Each candidate is loaded through Spandrel; only 1× image-restoration or 4× image-super-resolution models with three input and output channels appear. Unsupported, corrupt, other-scale, or non-RGB checkpoints are skipped. Older `Spandrel` folders are still searched for compatibility. The legacy **ESRGAN** selector remains separate and unchanged.

For example, [4× GameAI 2.0](https://openmodeldb.info/models/4x-GameAI-2-0) is a 4× RGB ESRGAN checkpoint. [1× DXTDecompressor Source V3](https://openmodeldb.info/models/1x-DXTDecompressor-Source-V3) is a 1× RGB ESRGAN model for removing DXT1 compression artifacts; it preserves image dimensions rather than enlarging them. Put either `.pth` file in `models`; if Spandrel recognizes the checkpoint, it appears in the same selector. For DDS textures, use TexConv to convert to PNG before the Spandrel stage and convert back to DDS afterward.

## 2. Install the inference runtime

Install **Python 3.10 or newer**. The selected Python must be on `PATH`, or `python.exe` must be beside AutoCrispy (or in a `python/` subfolder). If it is installed elsewhere, set the `AUTOCRISPY_PYTHON` environment variable to the full path to `python.exe`.

Install a PyTorch build appropriate for the computer. For NVIDIA GPU acceleration, first use the official [PyTorch install selector](https://pytorch.org/get-started/locally/) to install the CUDA-enabled build. Then, in that same Python environment, run:

```bat
python -m pip install "spandrel==0.4.2" Pillow
```

Spandrel 0.4.2 detects RealPLKSR-DySample, DAT architectures, ESRGAN models, and other supported architectures. AutoCrispy accepts 1× RGB models marked as Restoration and 4× RGB models marked as SR; it validates purpose and channels before inference. If CUDA is unavailable, it falls back to CPU; CPU processing will be much slower. The generic selector validates candidate files on startup, so checking a folder containing many large checkpoints can take a little time.

## 3. Use a Spandrel model

1. Start AutoCrispy. The **Spandrel** backend appears when at least one eligible checkpoint is found in `models` (or the legacy `Spandrel` folder).
2. Choose a model from the selector, then add it from the chain tab if needed. PLKSR V3 is selected by default when installed; otherwise DAT2 V4 is preferred, then the first eligible model.
3. Start the watcher as usual. The helper loads the selected checkpoint once per batch, then processes new textures.

A saved chain entry using the exact legacy model `4x_gameai_2.0` is upgraded to PLKSR V3 when that checkpoint is recognized, or DAT2 V4 otherwise if recognized. If neither is installed, the saved ESRGAN entry is left unchanged; other ESRGAN chains are also left alone.

The tile-size setting controls the maximum inference tile size. Start at `512`; increase it for fewer tiles/faster processing if there is enough GPU memory, or lower it if inference runs out of memory. Set it to `0` to try processing each image without tiling; on an out-of-memory error, AutoCrispy retries with 512-pixel tiles and then smaller tiles automatically. The CPU checkbox forces CPU inference. **DAT2 V4 is substantially slower and more memory-intensive than the PLKSR V3 option**, so tiled inference is recommended for large textures.

These are RGB image models, not normal/roughness/AO generators. A 1× model preserves the input dimensions. If an input has transparency, the runner processes RGB and resizes its alpha channel to the model output dimensions with Lanczos. Use AutoCrispy's existing alpha/defringe settings as needed.
