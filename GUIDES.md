# Setup Guides:

This will serve as a guide for common programs that support custom textures, and how to configure AutoCrispy to work with them.  **The information is not a definitive guide by any means, and neither presents the only or best way to accomplish these tasks!**

This is just how I've found to set these up, and it may be helpful to those who have never done something like this before.

  ## Quick Links:
  
  - [Dolphin](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#dolphin)
  - [PPSSPP](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#ppsspp)
  - [PJ64 - Under Construction](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#pj64)
  - [SpecialK - Under Construction](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#specialk)


  ## Dolphin [Top](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#quick-links)
  
  Dolphin does support both Dumping and Loading custom textures, but does not seem to check for *changes* in the custom textures whilst running. There is a workaround with Hotkeys.
  
  **TL;DR** Enable Texture Dumping. Set a hotkey for "Toggle Custom Textures". Setup and run AutoCrispy and the Game. Toggle every so often to update your textures.
  
  |Dolphin Setup|
  |----|
  |Open Dolphin's Graphics Settings|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/Dolphin01.png">|
  |Enable Both Texture Dumping and Loading Custom Textures|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/Dolphin02.png">|
  |Go to Hotkey Settings|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/Dolphin03.png">|
  |Add a Hotkey for Toggle Custom Textures. This can also be bound to a controller button.|
  <img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/Dolphin04.png">|
  |AutoCrispy's Input should be set to "\Documents\Dolphin Emulator\Dump\Textures\\\<GameID>"<br />AutoCrispy's Output should be set to "\Documents\Dolphin Emulator\Load\Textures\\\<GameID>"<br /><br />**Delete Input should be left Off!**|

  <br />
  
  ## PPSSPP [Top](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#quick-links)
  
  PPSSPP is great. It supports Dumping *and* Hotloading textures. All you need is some INI tweaks and you're golden.
  
  **TL;DR** Turn on Dumping and Loading textures. Make an INI with the settings below. Setup and run AutoCrispy and the Game. 
  
  |PPSSPP Setup|
  |----|
  |Open PPSSPP, and go to Settings|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/PPSSPP01.png">|
  |Under Tools, go to Developer Tools|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/PPSSPP02.png">|
  |Check Both Save New Textures and Replace Textures|
  |<img src="https://github.com/WalkerMx/DemoImages/blob/master/Guide_Assets/PPSSPP03.png">|
  
  Once you've started a game, you will be able to use the "Create/Open textures.ini file for current game" option. Alternatively, you can manually create a "textures.ini" file in the "\memstick\PSP\TEXTURES\\\<GameID>" folder.
  
  The contents of that file can be replaced with the following:
  
    [options]
    version = 1
    hash = xxh64
    ignoreAddress = true
    
  AutoCrispy's Input should be set to "\memstick\PSP\TEXTURES\\\<GameID>\new"<br />
  AutoCrispy's Output should be set to "\memstick\PSP\TEXTURES\\\<GameID>"

  **Delete Input can be turned on to save space.**
  
   <br />
  
  ## PJ64 [Top](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#quick-links)
  
  This is a Stub. I'm still Writing this.
  
  **TL;DR** Nothing here yet.
  
  |PJ64 Setup|
  |----|
  |This|
  |is|
  |under|
  |construction|
  
   <br />
  
  ## SpecialK [Top](https://github.com/WalkerMx/AutoCrispy/blob/master/GUIDES.md#quick-links)
  
  This is a Stub. I'm still Writing this.
  
  **TL;DR** Nothing here yet.
  
  |PJ64 Setup|
  |----|
  |This|
  |is|
  |under|
  |construction|
