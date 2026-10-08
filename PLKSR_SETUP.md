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

## Automatic Architect/Painter routing

The optional [NPPE3 Super Resolution ensemble](https://huggingface.co/divyanshgitmax/NPPE3-SuperResolution-Ensemble) describes **Architect** as suited to geometric/solid textures and **Painter** as suited to detailed/repeating textures. AutoCrispy's experimental router is available when the scan finds at least two different Spandrel-compatible 4× RGB super-resolution checkpoints in the shared `models` folder. Their filenames do not matter.

After refreshing the model scan, explicitly select **Auto texture routing · Architect / Painter** in the Spandrel model list. The two role dropdowns appear: choose a model for **Architect — solid textures** and a different model for **Painter — repeating textures**. The role choices are saved with AutoCrispy settings. Ordinary checkpoint choices do not invoke either routing role. If an old route entry is already in the chain, remove it there and add the ordinary model; chain entries are snapshots of the settings used when they were added.

By default, every batch has a strict Painter cap of **30%**, floored to a whole-texture count; for example, a 30% cap on three textures allows zero Painter assignments. Adjust the Painter cap and **minimum Painter score** in the Spandrel settings before running or adding the step to the chain (default score threshold: 0.34). A lower threshold makes more files eligible; the share remains a cap, so the router may send fewer than that percentage when too few files qualify. The router scores the whole pending batch, then processes the Architect group first and the Painter selections second, all in one run; you do not need to run the dump twice.

Click **Preview assignments...** to choose an input folder and inspect the actual textures in separate Painter and Architect lists. The preview shows each thumbnail, its feature score, the measured detail/edge/direction/pattern/repetition values, and the reason for its assignment (below threshold or outside the Painter share cap). It only analyzes files; it does **not** load either model or upscale/modify textures. It recursively scans supported image files in the same deterministic relative-path order as a normal run. The score formula is shown in the preview; the formula weights are fixed, while the minimum score and all-batch Painter cap are user-adjustable.

This is an experimental image-statistics heuristic, not a trained semantic classifier: it considers fine detail, edge density/direction, local-pattern diversity, and repetition, but cannot reliably know that a texture depicts grass, skin, stone, or brick. It can route a texture differently than expected; enable debug logging to inspect the scores. Each image is processed by one model (the models' predictions are **not** blended), so this is not the 75/25 full-image ensemble described by the model card. The Auto Texture Routing choice appears only when at least two different 4× RGB super-resolution models are recognized by Spandrel; no hard-coded checkpoint names are required. The router still needs a distinct pair because both roles must produce compatible 4× outputs. Model weights are not included in AutoCrispy.

## 2. Install the inference runtime

Install **64-bit Python 3.10 or newer**. The selected Python must be on `PATH`, or `python.exe` must be beside AutoCrispy (or in a `python/` subfolder). If it is installed elsewhere, set the `AUTOCRISPY_PYTHON` environment variable to the full path to `python.exe`.

For a guided setup, open **Advanced → Install / repair…** in AutoCrispy. It checks the Python executable AutoCrispy will use, then installs Spandrel 0.4.2, NumPy and Pillow; pip also installs PyTorch, TorchVision and their required dependencies. Model checkpoint weights are separate and are not downloaded by this installer.

For NVIDIA GPU acceleration, first use the official [PyTorch install selector](https://pytorch.org/get-started/locally/) to install the CUDA-enabled build into the same Python environment AutoCrispy uses. Then return to **Advanced → Install / repair…** (or run the command below) to install Spandrel and image dependencies. The dialog also links to Python downloads if Python 3.10+ is not installed.

```bat
python -m pip install "spandrel==0.4.2" Pillow numpy
```

Spandrel 0.4.2 detects RealPLKSR-DySample, DAT architectures, ESRGAN models, and other supported architectures. AutoCrispy accepts 1× RGB models marked as Restoration and 4× RGB models marked as SR; it validates purpose and channels before inference. If CUDA is unavailable, it falls back to CPU; CPU processing will be much slower. The generic selector validates candidate files on startup, so checking a folder containing many large checkpoints can take a little time.

## 3. Use a Spandrel model

1. Start AutoCrispy. The **Spandrel** backend appears when at least one eligible checkpoint is found in `models` (or the legacy `Spandrel` folder).
2. Choose a model from the selector, then add it from the chain tab if needed. PLKSR V3 is selected by default when installed; otherwise DAT2 V4 is preferred, then the first regular eligible model. **Auto Texture Routing is never the default**; select it explicitly to enable Architect/Painter routing.
3. Start the watcher as usual. The helper loads the selected checkpoint once per batch, then processes new textures. It reports per-image progress; if you run the Python helper directly, its output folder may be nested under the input folder—the nested output tree is excluded from discovery. The Python helper rejects identical input/output folders. For the GUI watcher, keep the input and output folder trees entirely separate (neither may contain the other), so output discovery and completion tracking remain unambiguous.

A saved chain entry using the exact legacy model `4x_gameai_2.0` is upgraded to PLKSR V3 when that checkpoint is recognized, or DAT2 V4 otherwise if recognized. If neither is installed, the saved ESRGAN entry is left unchanged; other ESRGAN chains are also left alone.

The tile-size setting controls the maximum inference tile size. On a 16 GB RTX 4080, `1024` is a good starting point; the default now uses that size and automatically retries with smaller tiles if GPU memory runs out. Older or lower-memory GPUs can use `512`. Set it to `0` to try processing each image without tiling; on an out-of-memory error, AutoCrispy retries with 512-pixel tiles and then smaller tiles automatically. AutoCrispy honors Spandrel's per-model tiling metadata: models marked `INTERNAL` receive the whole image and bypass external tiles and OOM tile retries; `DISCOURAGED` models remain usable with a warning; `SUPPORTED` models use the normal external-tile path. The Spandrel panel shows a color-coded tiling badge and a matching explanation; the Tile Size control is disabled when the selected model tiles internally. Auto Texture Routing shows the Architect and Painter tiling policies in the badge tooltip. CUDA is selected automatically when available; supported checkpoints use FP16 by default, with cuDNN autotuning and TF32 enabled for compatible CUDA paths. The **Precision** selector is available for Spandrel, PLKSR, and DAT2: **Auto** preserves this automatic choice; **FP16** requires CUDA and a checkpoint that advertises half-precision support; for unsupported models the selector switches back to Auto and marks that Auto uses FP32. Direct runner use rejects unsupported explicit FP16. **FP32** uses full float32 and disables TF32 on CUDA. The same choice is available to direct runner users with `--precision auto|fp16|fp32`. CPU operations use up to 16 PyTorch threads by default; direct runner users can override this with `--cpu-threads`. The CPU checkbox forces CPU inference and disables the precision selector; if FP16 was selected, it switches back to Auto because FP16 is not available on CPU. **DAT2 V4 is substantially slower and more memory-intensive than the PLKSR V3 option**, so tiled inference is recommended for large textures.

These are RGB image models, not normal/roughness/AO generators. A 1× model preserves the input dimensions. If an input has transparency, the runner processes RGB and resizes its alpha channel to the model output dimensions with Lanczos. Use AutoCrispy's existing alpha/defringe settings as needed.

For tangent-space normal-map textures only, Spandrel-family settings have an opt-in **Normal map** output mode (off by default): **Normalize XYZ** renormalizes all three decoded vector channels; **Rebuild Z from R/G** ignores blue and reconstructs positive Z, for BC5/two-channel or RG0 outputs. This is a pixel-vector cleanup after the model runs, not a normal-map-aware upscaler or a PBR map generator. The direct runner uses `--normal-map-mode none|normalize-xyz|rebuild-z`. Do not enable it for color textures, roughness, or AO maps.
