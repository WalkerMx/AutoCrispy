# AutoCrispy
Automatically apply AI upscaling on Dumped Textures.

## AutoCrispy Features

   - Seamless Upscaling - Experimental system for improving seamless texture upscales, based on [JoeyBallentine's](https://github.com/JoeyBallentine/ESRGAN) 'upscale.py' script.  Available for all backends.  For advanced users.  More information will be available in the [manual](https://github.com/WalkerMx/AutoCrispy/blob/master/MANUAL.md).

   - Model Chaining - Arbitrarily chain models for upscaling.  Adding a model to a chain 'snapshots' your current settings for the backend, meaning you can apply the same backend with differant settings or models (ESRGAN), or apply any mix of backends in sequence to your textures.

   - Defringing - Basic, GDI+ defringing scheme.  Removes ugly halo artifacts ('fringes') from textures with transparency.  This works best where all textures are roughly the same size.

## Backend Features

The following are the features supported in AutoCrispy. Any feature listed is not necessarily the same as the features or requirements of the program. To work, AutoCrispy requires one of the following to be downloaded, or ESRGAN to be installed.  <a href="https://github.com/WalkerMx/AutoCrispy/blob/master/COMPARE.md" target="_blank">More about the Backends and Comparison shots can be found here</a>

Backend|Scale Range|Denoising Support|Alpha Support|TAA|Custom Filters|Speed|VRAM Requirements|Download
:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:
Waifu2x Caffe|1-4|✔️|✔️|✔️|❌|Slow|Very High|[Link](https://github.com/lltcggie/waifu2x-caffe/releases)
Waifu2x Vulkan|1-2|✔️|✔️|✔️|❌|Average|Medium|[Link](https://github.com/nihui/waifu2x-ncnn-vulkan/releases)
RealSR Vulkan|4-4|❌|✔️|✔️|❌|*Slow*|High|[Link](https://github.com/nihui/realsr-ncnn-vulkan/releases)
SRMD Vulkan|2-4|✔️|✔️|✔️|❌|Average|Medium|[Link](https://github.com/nihui/srmd-ncnn-vulkan/releases)
Waifu2x CPP|1-8|✔️|✔️|❌|❌|Quick|Low|[Link](https://github.com/DeadSix27/waifu2x-converter-cpp/releases)
Anime4K CPP|1-8|❌|✔️|❌|✔️|Quick|Low|[Link](https://github.com/TianZerL/Anime4KCPP)

## Texconv

AutoCrispy also supports <a href="https://github.com/Microsoft/DirectXTex/wiki/Texconv" target="_blank">Microsoft's Texconv Utility</a> for processing DDS textures. Most DDS formats are supported, as well as many image formats.  When chaining, ensure that the backends you have selected support the format you have chosen (PNG is widely supported).

To use texconv, place it in the folder with AutoCrispy, or in it's own folder in AutoCrispy's folder. To set up for DDS files, set the first item in your chain to Texconv, set to "DDS Input". Then, set the last item to Texconv, set to "DDS Output".

## ESRGAN

It is also possible to use ESRGAN. For convenience, a prebuilt, portable installation can be found here:<br />
https://github.com/WalkerMx/ESRGAN_Python_Embedded/releases

This does not include any models. [They can be found here.](https://upscale.wiki/wiki/Model_Database)

Alternatively, the setup guide can be found here:<br />
https://upscale.wiki/wiki/ESRGAN_Installation_Guide_for_Windows

## GFPGAN

Recently, Support was added for GFPGAN.  This feature requires the latest Embedded Python Build:<br />
https://github.com/WalkerMx/ESRGAN_Python_Embedded/releases

Also Required is the GFPGAN Model:<br />
https://github.com/TencentARC/GFPGAN/releases/download/v1.3.0/GFPGANv1.3.pth

Place the Model in the 'models' folder, inside the 'esrgan' folder.  When using AutoCrispy, Select Python as the Backend, 'inference_gfpgan.py' as the Script, and 'GFPGANv1.3.pth' as the model.

Note that the 'inference_gfpgan.py' from the Embedded Python Build has been modified to work with AutoCrispy! 

## How to use:
    -Toss AutoCrispy into the folder from the backend(s) you chose, and run it.
  
    -Pick the folder where the textures dump.
  
    -Pick the folder where the new textures need to be.
  
    -Set your settings. The defaults should work, unless you know what you want.
  
    -Push the button.
  
  
  As the textures dump, new upscaled textures are generated, and the originals are optionally deleted.
  
  Also, make sure you have enough VRAM if you use multithreading, especially with Caffe and ESRGAN. More threads & higher upscales dramatically increase the memory requirements!
  
  More info about AutoCrispy and how it works can be found [Here](https://github.com/WalkerMx/AutoCrispy/blob/master/MANUAL.md)<br />
  Guides for some common programs can be found [Here](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md).


# Special Thanks

Some people have been unusually helpful, or have gone above and beyond to show support for this project.  With their permission, they are listed below.

### u/devilskin
### fefo-dev
