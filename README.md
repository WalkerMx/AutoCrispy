# AutoCrispy
Automatically Run Waifu2x on Dumped Textures

Requires at least one of the following:

- Waifu2x-converter-cpp: https://github.com/DeadSix27/waifu2x-converter-cpp/releases
- Waifu2x-Caffe: https://github.com/lltcggie/waifu2x-caffe/releases
- Waifu2x-ncnn-vulkan: https://github.com/nihui/waifu2x-ncnn-vulkan/releases

Latest AutoCrispy Release Here: https://github.com/WalkerMx/AutoCrispy/releases

# How to use:
    -Toss AutoCrispy into the Waifu2x folder, and run it.
  
    -Pick the folder where the textures dump.
  
    -Pick the folder where the new textures need to be.
  
    -Set your settings. The defaults should work, unless you know what you want.
  
    -Push the button.
  
  
  As the textures dump, new Waifu2x'd textures are generated, and the originals are optionally deleted.
  
  Also, make sure you have enough VRAM if you use multithreading. More threads & higher upscales dramatically increase the memory requirements!
  
# About Backends:
   Each of the backends have their own characteristics and quirks when used with AutoCrispy:
   
   - **Waifu2x-converter-cpp** is built using OpenCV. When doing actual upscaling, it uses very little CPU and GPU resources. It allows for variable amounts of compression to be applied to the output. It has many options available for output, and can be used even on lower-end mid-tier machines. This works best with Half or All Threads. This is the fastest, but tends to have pretty bad fringes on images with transparency.
   
   - **Waifu2x-Caffe** is intended for nVidia GPUs with CUDA, and has the highest VRAM memory requirements of the three. It has a decent amount of options. This runs the slowest, and has high requirements. It's output tends to looks the nicest.
    
   - **Waifu2x-ncnn-vulkan** is designed to run on (what else?) Vulkan. It can run on AMD and nVidia GPUs that support Vulkan. It has few options, and can be picky about your Vulkan interface, but once working, you can get very good speed coupled with a much lower VRAM overhead. This tends to be the middle ground. Faster than Caffe, but limited to 2x resolution. Simulating higher resolutions (by running the textures through again) results in minor fringing.
   

# Reference Images:
Images for comparison. The source is not mine, and these images exist purely for edification.
   
Caffe 2x | Vulkan 2x 
---------|----------
---------|----------
<img class="header" src="https://github.com/WalkerMx/AutoCrispy/blob/master/Reference_Assets/caffe2x.png">|<img src="https://github.com/WalkerMx/AutoCrispy/blob/master/Reference_Assets/vulkan2x.png">
---------|----------
<img src="https://github.com/WalkerMx/AutoCrispy/blob/master/Reference_Assets/cpp2x.png">|<img src="https://github.com/WalkerMx/AutoCrispy/blob/master/Reference_Assets/xBRX2x.png">
  CPP 2x | xBRZ 2x
  

Caffe 4x | Vulkan 4x (Simulated)
---------|----------
---------|----------
<img src="https://github.com/WalkerMx/AutoCrispy/blob/master/Reference_Assets/caffe4x.png">|<img src="https://github.com/WalkerMx/AutoCrispy/blob/master/Reference_Assets/vulkan4x sim.png">
---------|----------
<img src="https://github.com/WalkerMx/AutoCrispy/blob/master/Reference_Assets/cpp4x.png">|<img src="https://github.com/WalkerMx/AutoCrispy/blob/master/Reference_Assets/xBRZ4x.png">
  CPP 4x | xBRZ 4x
  
# Notes:
    -Works great for PPSSPP. Make a textures.ini with these options set:
      hash = xxh64
      ignoreAddress = true
      
    -There's no reason you can't use this with other programs, eg. Dolphin.
      The programs need to support custom textures
      The programs need to support hotloading textures (load custom textures without a restart)
