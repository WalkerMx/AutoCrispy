# AutoCrispy — persistent development handoff

_Last updated: 2026-10-07 (Europe/Prague). Read this before continuing repository work._

## Repository and branch

- Repository: `berlint-hub/AutoCrispy`
- Arena work must stay on `arena/1924e580-autocrispy`; do not switch to, create, or push another branch, and never push these changes to `master`.
- The route-preview/tuning implementation is on `8865f25` (`Fix route preview Painter count compilation`); Windows Actions passed: [37652638723](https://github.com/berlint-hub/AutoCrispy/actions/runs/37652638723). The previous handoff-note base was `310f327`.
- The user has authorized pushing their changes to this Arena branch to trigger GitHub Actions. Do not create an issue for an audit; if offering code upstream, ask the maintainer whether they want to use it.

## Completed work

### Spandrel support

- Added a standalone `spandrel_upscale.py` runner and checkpoint discovery, separate from the legacy ESRGAN backend.
- Present Spandrel as the selectable backend for this model group. PLKSR and DAT2 are model choices within Spandrel, not separate backends. Keep all weights in the shared `models` folder; older `Spandrel` folders remain supported.
- Support discovered compatible 1× RGB restoration and 4× RGB super-resolution checkpoints. Do not add the full PBRify model set or map-generation workflow.
- Keep tile size `0` support (try full-image inference, then retry with smaller tiles on an out-of-memory error) and progress reporting that includes input/output basenames.
- Keep legacy ESRGAN separate from generic Spandrel. Model files/runtime are installed by the user, not committed.

### Auto Texture Routing

- Added feature-based per-texture Architect/Painter routing. It is heuristic image-statistics routing, not semantic object recognition; one model handles each image and outputs are not blended.
- In commit `6ee05abc`, made routing opt-in and added role dropdowns visible only when **Auto texture routing · Architect / Painter** is selected:
  - **Architect — solid textures**
  - **Painter — repeating textures**
- The role selectors use discovered compatible 4× RGB SR models; hard-coded `best_realesrnet` / `best_swinir` filenames are not required. A route needs two different checkpoints because the runner requires a distinct 4× pair. The chosen roles are saved with settings.
- The panel exposes a **minimum Painter score** (default `0.34`) and a Painter share cap (default `30%`, used for batches of 10+). Both are saved with settings and included in chain snapshots; the score formula weights remain fixed.
- **Preview assignments...** runs feature analysis only: it does not load either upscaler, write output, or modify textures. The dialog groups actual thumbnails into Painter/Architect lists and shows score, detail, edge density, direction entropy, local-pattern entropy, periodicity, and why the route was selected (below threshold or outside the Painter cap). It analyzes the same top-level supported image files as a run.
- The runner exposes this no-upscale path as `--preview-route`; its machine-readable results are consumed by the WinForms dialog.
- A regular selected checkpoint must use the single-model command path; the `--auto-route` flag is emitted only for a package with `AutoRouteEnabled=True`.
- **Chain behavior matters:** chain entries snapshot their settings. Changing the main model selector does not rewrite an already-added Auto Texture Routing entry. To run only a regular model, remove the old Auto-routing chain item and add the regular checkpoint. New route entries show their selected role filenames in the chain label.

### Paths, queue, and cancellation

- Added persistent input/output path profiles for multiple games, with checkboxes controlling which profiles the watcher/upscaler uses; old single input/output settings migrate to a profile.
- Commit `279e2a3` added persistent game input/output path profiles; queue cancellation also requests `BackgroundWorker` cancellation, kills registered active processes, cleans temporary work, and restores the UI to `Running: False` after cancellation.
- **Open issue reported by the user on 2026-10-07:** “When I want to stop, it keeps running.” Do not assume cancellation is fixed just because the current UI completion handler sets `Running: False`; verify that the actual backend/Python process and output writes stop. The current code calls `CancelAsync()` and `StopActiveProcesses()`, tracks active `Process` objects, and uses `Process.Kill()`; cancellation is checked while waiting and between pipeline stages. The latest user report has not yet been reproduced or fixed.
- Investigate whether the stop click is stopping only the watcher or an active `WorkHorse`, whether an external process tree/child survives killing its parent, and whether work is in a long non-interruptible preprocessing step. Confirm behavior for the user's Auto-routing/Spandrel path as well as other backends if possible. A process-tree termination strategy may be needed; this is a hypothesis, not a confirmed root cause. Preserve queue cleanup and the final `Running: False` state.

## Verification already completed

- For the route-preview/tuning work, `python -m unittest discover -s AutoCrispy/AutoCrispy/tests -v`: **16 tests passed**; Python `py_compile` passed for the runner and tests. The Windows build and runner tests passed for `8865f25`: [run 37652638723](https://github.com/berlint-hub/AutoCrispy/actions/runs/37652638723).
- The prior configurable-routing commit passed 13 Python tests and its Windows build; see the action link below.
- GitHub Actions Windows build and runner tests passed for `6ee05abc`: [run 37645474245](https://github.com/berlint-hub/AutoCrispy/actions/runs/37645474245).
- The prior path-profile commit `279e2a3` also passed Actions: [run 37639182091](https://github.com/berlint-hub/AutoCrispy/actions/runs/37639182091).
- The Windows CI build is the available compile check for the VB.NET WinForms application; the local Linux workspace does not have MSBuild/.NET installed.

## User constraints to preserve

- Do not integrate the full PBRify model set or map-generation workflow.
- Keep one shared `models` folder; keep Spandrel separate from legacy ESRGAN.
- PLKSR and DAT2 remain model choices in the Spandrel selector.
- Preserve 1× and 4× support, tile size `0`, input/output-basename progress, profile checkboxes, and cancellation behavior.
- Continue working and pushing only to `arena/1924e580-autocrispy`.

## Related PCSX2 troubleshooting, if the user returns to it

- PCSX2 2.9 texture replacement status indicator: **Settings → Graphics → On-Screen Display → Show Texture Replacement Status**; the OSD reports dump/replacement counts.
- The reported Gran Turismo black replacement textures were not diagnosed conclusively. Missing mipmaps are more likely to affect distant/LOD sampling than to make the base texture black at all distances. Check replacement matching/name/game serial, output validity, and alpha; AutoCrispy's **Fix PS2** path removes/restores PS2 alpha around the first/final processing stages. Test one texture with the setting on/off rather than batch-changing the pack.
