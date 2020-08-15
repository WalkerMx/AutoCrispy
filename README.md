# AutoCrispy
Automatically AI upscaling on Dumped Textures. Comparison shots are [here](https://github.com/WalkerMx/AutoCrispy/blob/master/COMPARE.md).

## Backend Features

The following is the features supported in AutoCrispy. And feature listed is not necessarily the same as the features or requirements of the program. To work, AutoCrispy requires one of the following (or ESRGAN, requires setup).

Backend|Scale Range|Denoising Support|Alpha Support|TAA|Custom Filters|Speed|Quality|Download
:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:
Waifu2x Caffe|1-4|✔️|✔️|✔️|❌|Slow|Great|[Link](https://github.com/lltcggie/waifu2x-caffe/releases)
Waifu2x Vulkan|1-2|✔️|✔️|✔️|❌|Average|Good|[Link](https://github.com/nihui/waifu2x-ncnn-vulkan/releases)
RealSR Vulkan|4-4|❌|✔️|✔️|❌|Slow|Good|[Link](https://github.com/nihui/realsr-ncnn-vulkan/releases)
SRMD Vulkan|2-4|✔️|✔️|✔️|❌|Average|Good|[Link](https://github.com/nihui/srmd-ncnn-vulkan/releases)
Waifu2x CPP|1-8|✔️|✔️|❌|❌|Quick|OK|[Link](https://github.com/DeadSix27/waifu2x-converter-cpp/releases)
Anime4K CPP|1-8|❌|✔️|❌|✔️|Quick|Good|[Link](https://github.com/TianZerL/Anime4KCPP)

## ESRGAN

It is also possible to use ESRGAN. This requires the setup of several applications:

 - Python: https://www.python.org/downloads/
 - PyTorch (pip): https://pytorch.org/get-started/locally/
 - PyOpenCL (pip): https://pypi.org/project/pyopencl/
 - A fork of ESRGAN: https://github.com/xinntao/ESRGAN/network/members
 - An ESRGAN Model: https://upscale.wiki/wiki/Model_Database

A setup guide for ESRGAN can be found here: https://upscale.wiki/wiki/ESRGAN_Installation_Guide_for_Windows

# How to use:
    -Toss AutoCrispy into the folder from the backend(s) you chose, and run it.
  
    -Pick the folder where the textures dump.
  
    -Pick the folder where the new textures need to be.
  
    -Set your settings. The defaults should work, unless you know what you want.
  
    -Push the button.
  
  
  As the textures dump, new upscaled textures are generated, and the originals are optionally deleted.
  
  Also, make sure you have enough VRAM if you use multithreading, especially with Caffe and ESRGAN. More threads & higher upscales dramatically increase the memory requirements!
  
  Guides for some common programs can be found [Here](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md).
