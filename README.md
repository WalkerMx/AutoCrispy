# AutoCrispy
Automatically Run Waifu2x-Caffe on Dumped Textures

Requires Waifu2x-Caffe: https://github.com/lltcggie/waifu2x-caffe/releases

Latest Release Here: https://github.com/WalkerMx/AutoCrispy/releases

# How to use:
    -Toss AutoCrispy into the Waifu2x-Caffe folder, and run it.
  
    -Pick the folder where the textures dump.
  
    -Pick the folder where the new textures need to be.
  
    -Set your settings. The defaults should work, unless you know what you want.
  
    -Push the button.
  
  
  As the textures dump, new Waifu2x'd textures are generated, and the originals are optionally deleted.
  
  # Notes:
    -Works great for PPSSPP. Make a textures.ini with these options:
      hash = xxh64
      ignoreAddress = true
      
    -There's no reason you can't use this with other programs, eg. Dolphin.
      The programs need to support custom textures
      The programs need to support hotloading textures (load custom textures without a restart)
