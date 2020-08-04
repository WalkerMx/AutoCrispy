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
  
  # Notes:
    -Works great for PPSSPP. Make a textures.ini with these options set:
      hash = xxh64
      ignoreAddress = true
      
    -There's no reason you can't use this with other programs, eg. Dolphin.
      The programs need to support custom textures
      The programs need to support hotloading textures (load custom textures without a restart)
      
    -Last, make sure your system can run Waifu2x.
      CPU processing is Super Slow!
      GPU/cuDNN works good if your program isn't GPU heavy.
      Won't run on a potato.  Two networked potatoes could probaby do it though.
