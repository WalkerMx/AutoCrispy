# AutoCrispy
Automatically apply AI upscaling on Dumped Textures. Comparison shots are [here](https://github.com/WalkerMx/AutoCrispy/blob/master/COMPARE.md).

## AutoCrispy Features

   - Model Chaining - Arbitrarily chain models for upscaling.  Adding a model to a chain 'snapshots' your current settings for the backend, meaning you can apply the same backend with differant settings or models (ESRGAN), or apply any mix of backends in sequence to your textures.  Chains can be saved, but the paths are absolute - if you move AutoCrispy to a new folder, you will need to rebuild your chains!

   - Defringing - Basic, GDI+ defringing scheme.  Removes ugly halo artifacts ('fringes') from textures with transparency.  This works best where all textures are roughly the same size.

## Backend Features

The following are the features supported in AutoCrispy. Any feature listed is not necessarily the same as the features or requirements of the program. To work, AutoCrispy requires one of the following to be downloaded, or ESRGAN to be installed.

Backend|Scale Range|Denoising Support|Alpha Support|TAA|Custom Filters|Speed|Quality|Download
:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:
Waifu2x Caffe|1-4|✔️|✔️|✔️|❌|Slow|Great|[Link](https://github.com/lltcggie/waifu2x-caffe/releases)
Waifu2x Vulkan|1-2|✔️|✔️|✔️|❌|Average|Good|[Link](https://github.com/nihui/waifu2x-ncnn-vulkan/releases)
RealSR Vulkan|4-4|❌|✔️|✔️|❌|*Slow*|Great|[Link](https://github.com/nihui/realsr-ncnn-vulkan/releases)
SRMD Vulkan|2-4|✔️|✔️|✔️|❌|Average|Good|[Link](https://github.com/nihui/srmd-ncnn-vulkan/releases)
Waifu2x CPP|1-8|✔️|✔️|❌|❌|Quick|OK|[Link](https://github.com/DeadSix27/waifu2x-converter-cpp/releases)
Anime4K CPP|1-8|❌|✔️|❌|✔️|Quick|Good|[Link](https://github.com/TianZerL/Anime4KCPP)

## ESRGAN

It is also possible to use ESRGAN. For convenience, a prebuilt, portable installation can be found here:<br />
https://github.com/WalkerMx/ESRGAN_Python_Embedded/releases

This does not include any models. [They can be found here.](https://upscale.wiki/wiki/Model_Database)

Alternatively, the setup guide can be found here:<br />
https://upscale.wiki/wiki/ESRGAN_Installation_Guide_for_Windows

## How to use:
    -Toss AutoCrispy into the folder from the backend(s) you chose, and run it.
  
    -Pick the folder where the textures dump.
  
    -Pick the folder where the new textures need to be.
  
    -Set your settings. The defaults should work, unless you know what you want.
  
    -Push the button.
  
  
  As the textures dump, new upscaled textures are generated, and the originals are optionally deleted.
  
  Also, make sure you have enough VRAM if you use multithreading, especially with Caffe and ESRGAN. More threads & higher upscales dramatically increase the memory requirements!
  
  Guides for some common programs can be found [Here](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md).


# Special Thanks

Some people have been unusually helpful, or have gone above and beyond to show support for this project.  With their permission, they are listed below.

### u/devilskin
