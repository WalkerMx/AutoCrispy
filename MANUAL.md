# Manual
AutoCrispy has some features.  I'll explain them here.

## Files

AutoCrispy will try to detect any Executables it supports on startup.  They can be in the folder with AutoCrispy, or in their own subfolders.  Note that, AutoCrispy only searches one folder deep (that is to say, it doesn't check any folders inside of folders for the Executables).

AutoCrispy also searches in these subfolders for Python scripts and ESRGAN models. For ESRGAN models, it searches two directory levels down. Put Spandrel checkpoints in one shared `models` folder under the program or configured backend root. PLKSR V3 and DAT2 V4 appear in the single Spandrel model selector when recognized; there are no separate PLKSR/DAT2 backend choices. The selector lists Spandrel-recognized 1× RGB restoration or 4× RGB super-resolution models. Older `Spandrel` folders remain supported. This selector is separate from the legacy ESRGAN backend. See [PLKSR_SETUP.md](PLKSR_SETUP.md) for folder and Python/PyTorch setup. When at least two compatible 4× RGB models are found, the selector offers **Auto texture routing**. The role dropdowns let you choose any eligible model for **Architect — solid textures** and **Painter — repeating textures**; model filenames do not need to be special. Routing is opt-in: selecting a regular model runs only that checkpoint. In Auto Texture Routing, set the Painter share cap (used for batches of 10+) and minimum feature score, then use **Preview assignments...** to see per-texture thumbnails, role, score, feature values, and assignment reason without running either upscaler. Details and limitations are in [PLKSR_SETUP.md](PLKSR_SETUP.md).

## Multiple PCSX2 game folders

Open the **Paths** tab and choose **Manage game paths...** to save input/output folder pairs for your games. Add one profile per game (for example, `SLUS-20111`), choose its input and replacement output folders, and check the rows AutoCrispy should watch. The watcher scans the checked profiles and processes pending games sequentially; the progress display aggregates their completion. Profiles are saved with AutoCrispy settings. Existing single input/output settings are migrated into a checked profile automatically, and **Run Once** remains a separate one-image operation.

## Special Notes about Python

AutoCrispy does its best to detect Python installations.  First, it checks for the portable Python build.  Next, it will attempt to read PATH to see if Python is installed somewhere on the system, and if so, gets its path.

Otherwise, AutoCrispy will try to send Python commands directly through the shell (sometimes this works even without PATH set, somehow).  Be aware, that if Python runs commands via the shell, Output Logging will be Disabled, and you will see the Python Interpreter open in your taskbar, minimized.

The Spandrel model runner is separate from the legacy ESRGAN Python bundle. It needs Python 3.10 or newer with PyTorch, `spandrel==0.4.2`, Pillow, and NumPy. It looks for `python.exe` on PATH or beside AutoCrispy; set `AUTOCRISPY_PYTHON` to the full executable path for a custom install. See [PLKSR_SETUP.md](PLKSR_SETUP.md) for checkpoint and GPU setup.

## Chains

AutoCripsy supports chaining models together.  This is agnostic of process and platform; you can chain any of the backends together, with any other backend, and with any script and/or model from ESRGAN.

To add a model to the chain, first set the settings you'd like to add to the chain in the UI.  Then, under "Chaining", click the "Add" button.  This "snapshots" your settings, and adds them to the chain. Changing the model selector later does not change an existing chain item; remove an old Auto Texture Routing item and add the regular model if you want to stop routing.  Chain objects can be re-arranged, by clicking and draging them.  Be aware that only ~5 models should be added to the chain (after this, you will encounter strange/unexpected behaviour.  You can make longer chains, using a text editor on an existing saved chain, and they will work, however the UI will misbehave if you try to edit it).

In addition to adding, re-arranging, and removing, you can also Edit and Rename models in the chain.  This is done through the Right Click -> Edit menu.  This will bring up an XML representation of the chain object, letting you edit its Name, Icon, and internal settings.

Chains can also be saved and loaded.  One important note, is that both the saves and AutoCrispy rely on the folder structure of your backends to function; if you move the files around, or change the folder names, your chains will have to be remade for them to function (the backends will "error out", because the files aren't found).  This issue has been addressed, by making the internal folder structures *relative*.  This means that, you can move the folder that AutoCrispy is in, and everything should still work (i.e. if you put AutoCrispy and the backends on a flash drive or external hdd, or if you move everything from your desktop to your Documents, everything should still work.)

The last thing to note, is that if no models are in the chain (i.e. the "Chaining" tab is empty), AutoCrispy will simply use whatever settings you have currently set in the UI.

## Seamless Modes

Textures that are intended to be seamless sometimes lose this quality when they are upscaled, resulting in ugly seams in-game.  This feature aims to correct this.

What's actually happening, is AutoCrispy is taking the image, and tiling it in a 3x3 grid (for a total of 9 copies of the image).  Then, to conserve compute time, the image is trimmed, leaving a predetermined margin around the image.  Once this process is complete, AutoCrispy passes the image to the backend for upscaling.

After the upscaing is done, the margins (which now contains the edge artifacts) are trimmed off, resulting in a cleaner image.

The 'flipped' option is very similar, with one exception - instead of simply tiling the images, the images are reflected from the center. This removes hard edges from the grid, and can result in better overall upscale quality.

The Seam Scale setting should be set to the final upscale value.  If you are only upscaling 4x, then this value would be 4.  If you have a chain, the value will need to be calculated as the product off all scales in the chain; for example, if you had a chain like 1x - 3x - 4x, you would calculate 1\*4\*3 = 12, and so 12 would be your value.

The Seam Margin setting determines how wide the margins about the image are.  Setting this lower than 4 is not recommended.  The higher the value, the more computer resources it will take to process the same image, however the results may be better.  Do not set this value higher than the width of the original image.

## Defringing

Some backends don't have the nicest output, when fed images with Alpha.  To fix this, provided is a *basic* GDI+ defringing scheme; what it does, is look at each Alpha byte, and make it fully transparent if it isn't above a certain threshold (user set; varies). Otherwise, it leaves the Alpha byte alone.

There are more complex methods available, which are being looked into, however this feature was decided on due to its *speed*.  While it might take some fine tuning to get the results you want, once it is set, the process is quite fast.
