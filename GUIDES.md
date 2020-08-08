# Baisc How-to's

This will serve as a guide for common programs that support custom textures, and how to configure AutoCrispy to work with them.  **The information is not a definitive guide by any means, and neither presents the only or best way to accomplish these tasks!**

This is just how I've found to set these up, and it may be helpful to those who have never done something like this before.

  ## Quick Links:
  
  - [Dolphin](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#dolphin)


  ## Dolphin
  
  Dolphin does support both Dumping and Loading custom textures, but does not seem to check for *changes* in the custom textures whilst running. There is a workaround with Hotkeys.
  
  **TL;DR** Enable Texture Dumping. Set a hotkey for "Toggle Custom Textures". Setup and run AutoCrispy and the Game. Toggle every so often to update your textures.
  
  |This|
  |----|
  |Open Dolphin's Graphics Settings|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/Dolphin01.png">|
  |Enable Both Texture Dumping and Loading Custom Textures|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/Dolphin02.png">|
  |Go to Hotkey Settings|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/Dolphin03.png">|
  |Add a Hothey for Toggle Custom Textures. This can also be bound to a controller button, like the unused left bumper on many modern controllers.|
  <img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/Dolphin04.png">|
  |AutoCrispy's Input should be set to "\Documents\Dolphin Emulator\Dump\Textures\<GameID>"<br />AutoCrispy's Output should be set to "\Documents\Dolphin Emulator\Load\Textures\<GameID>"<br /><br />**Delete Input should be left Off.**|

  <br />
  
  ## PPSSPP
  
  PPSSPP is great. It supports Dumping *and* Hotloading textures. All you need is some INI tweaks and you're golden.
  
  **TL;DR** Turn on Dumping and Loading textures. Make an INI with the settings below. Setup and run AutoCrispy and the Game. 
  
  This|Is|Where|The|Picture|Guide|Will|Go
  ----|--|-----|---|-------|-----|----|--  
  ----|--|-----|---|-------|-----|----|[options]<br/>version = 1<br/>hash = xxh64<br/>ignoreAddress = true
