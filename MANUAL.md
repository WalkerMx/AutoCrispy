# Manual (UNDER CONSTRUCTION)

AutoCrispy has some features.  I'll explain them here.

## Files

AutoCrispy will try to detect any Executables it supports on startup.  They can be in the folder with AutoCrispy, or in their own subfolders.  Note that, AutoCrispy only searches one folder deep (that is to say, it doesn't check any folders inside of folders for the Executables).

Autocrispy also searches in these subfolders for Python Scrips and ESRGAN Models.  For ESRGAN Models, AutoCrispy searches down one level further (So, two directorys down).  The intent, is to have a folder for ESRGAN, with the scripts inside it, and a folder with your models inside that (and, if you have the portable build provided, that in the folder with the scripts as well.)

## Chains

AutoCripsy supports chaining models together.  This is agnostic of process and platform; you can chain any of the backends together, with any other backedn, and with any script and/or model from ESRGAN.

To add a model to the chain, first set the settings you'd like to add to the chain in the UI.  Then, under "Chaining", click the "Add" button.  This "snapshots" your settings, and adds them to the chain.  Chain objects can be re-arranged, by clicking and draging them.  Be aware that only ~5 models should be added to the chain (after this, you will encounter strange/unexpected behaviour.  You can make longer chains, using a text editor on an existing saved chain, and they will work, however the UI will misbehave if you try to edit it).

In addition to adding, re-arranging, and removing, you can also Edit and Rename models in the chain.  This is done through the Right Click -> Edit menu.  This will bring up an XML representation of the chain object, letting you edit its Name, Icon, and internal settings.

Chains can also be saved and loaded.  One important note, is that both the saves and AutoCrispy rely on the folder structure of your backends to function; if you move the files around, or change the folder names, your chains will have to be remade for them to function (the backends will "error out", because the files aren't found).  This issue has been addressed, by making the internal folder structures *relative*.  This means that, you can move the folder that AutoCrispy is in, and everything should still work (i.e. if you put AutoCrispy and the backends on a flash drive or external hdd, or if you move everything from your desktop to your Documents, everything should still work.)

## Defringing

Some backends don't have the nicest output, when fed images with Alpha.  To fix this, provided is a *basic* GDI+ defringing scheme; what it does, is look at each Alpha byte, and make it fully transparent if it isn't above a certain threshold (user set; varies). Otherwise, it makes the Alpha byte fully opaque.

There are more complex methods available, which are being looked into, however this feature was decided on due to its *speed*.  While it might take some fine tuning to get the results you want, once it is set, the process is quite fast.