# More about Backends

Backend|About
:-:|:-:
Waifu2x Caffe|Waifu2x Caffe does great with Anime and Manga style images.  Both the Image output quality and the Alpha quality are top-notch, on the appropriate image type.
Waifu2x Vulkan|Waifu2x Vulkan approaches the quality of Caffe, while slashing the computational and memory requirements.  The image quality is good, but the Alpha handling can be a little fuzzy around the edges (can be defringed to great effect.)
RealSR Vulkan|RealSR's image output is great for more realistic images.  The Alpha can be a little dark around the edges, but the detail produced on images is fantastic.
SRMD Vulkan|SRMD excels at upscaling poor-quality material.  Great for reworking a bad upscale, and also good for upscaling realistic images.
Waifu2x CPP|Waifu2x CPP's output is visually close to Caffe's (if, a little muddier), but is quite fast with low requirements.  It does poorly on images with Alpha (but the results are acceptable with defringing turned on and calibrated).
Anime4K CPP|Anime4K CPP is the fastest of the supported backends.  It has a high range of input settings, and is intended for Anime or Manga style images.  Visual quality has quite a bit of variance, but can look quite good once the settings are dialed in.

# Reference Images:
Images for comparison. The source is not mine, and these images exist purely for edification.

|Source|
|---|
|<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/Sample.png">|
   
Caffe 4x|Waifu2x Vulkan 4x
---|---
<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/Caffe4x.png">|<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/WaifuVulkan4x.png">

Waifu2x CPP 4x|Anime4K 4x
---|---
<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/WaifuCPP4x.png">|<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/Anime4K4x.png">

RealSR 4x|SRMD 4x
---|---
<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/RealSR4x.png">|<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/SRMD4x.png">

ESRGAN (4xBox)|ESRGAN (4x_FireAlpha)
---|---
<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/ESRGAN4xBox.png">|<img src="https://github.com/WalkerMx/DemoImages/blob/master/References/ESRGAN4xFireAlpha.png">
