# AutoCrispy
Automatically Run Waifu2x on Dumped Textures

Requires at least one of the following:

- Waifu2x-Caffe: https://github.com/lltcggie/waifu2x-caffe/releases
- Waifu2x-ncnn-vulkan: https://github.com/nihui/waifu2x-ncnn-vulkan/releases
- Waifu2x-converter-cpp: https://github.com/DeadSix27/waifu2x-converter-cpp/releases

Latest AutoCrispy Release Here: https://github.com/WalkerMx/AutoCrispy/releases

# How to use:
    -Toss AutoCrispy into the Waifu2x folder, and run it.
  
    -Pick the folder where the textures dump.
  
    -Pick the folder where the new textures need to be.
  
    -Set your settings. The defaults should work, unless you know what you want.
  
    -Push the button.
  
  
  As the textures dump, new Waifu2x'd textures are generated, and the originals are optionally deleted.
  
# About Backends:
   Each of the backends have their own characteristics and quirks when used with AutoCrispy:
   
   - **Waifu2x-Caffe** is intended for nVidia GPUs with CUDA, and has the highest VRAM memory requirements of the three. It has a decent amount of options, and can run at a decant speed, depending on your hardware.
    
   - **Waifu2x-ncnn-vulkan** is designed to run on (what else?) Vulkan. It can run on AMD and nVidia GPUs that support Vulkan. It has few options, and can be picky about your Vulkan interface, but once working, you can get very good speed coupled with a much lower VRAM overhead.
    
   - **Waifu2x-converter-cpp** is built using OpenCV. When doing actual upscaling, it uses very little CPU and GPU resources. It allows for variable amounts of compression to be applied to the output. It has many options available for output. Currently, it isn't notably faster than the other two.
    
# Notes:
    -Works great for PPSSPP. Make a textures.ini with these options set:
      hash = xxh64
      ignoreAddress = true
      
    -There's no reason you can't use this with other programs, eg. Dolphin.
      The programs need to support custom textures
      The programs need to support hotloading textures (load custom textures without a restart)
