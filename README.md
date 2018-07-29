# UnityTextureAtlasCreator

A simple tool to easily combine materials on a single texture atlas and material, useful to improve batching and reduce draw calls on mobile games.

## How it works
* All textures are combined on a texture atlas using [Unity's built-in PackTextures method](https://docs.unity3d.com/ScriptReference/Texture2D.PackTextures.html)
* A new material that uses the texture atlas is created.
* New UV's are calculated and applied to each selected mesh. Currently it is not saving the new meshes on assets, but it should be easy to do. :)

## How to use
* Add the Editor folder to your project.
* Select the GameObject that uses the materials that you want to combine. 

![](Screenshots/SelectObjects.PNG?raw=true)
* Click on Window/TextureAtlasCreator to open the tool window, you can see all textures that can be combined on an atlas and also change the max size of the generated atlas.

![](Screenshots/CreateAtlasWindow.PNG?raw=true)

* Click on "Create Texture Atlas" and wait a few seconds. You will be asked for paths to save the atlas texture and material. After some time you will see a window showing the generated atlas and all the selected meshes will be updated with new UVs and a new material that uses it.

![](Screenshots/AtlasPreview.PNG?raw=true)

## Current issues
* Only works with materials using Mobile/Diffuse shader (actually, with a minor change it should work with any materials that uses a single texture material)
* Can't handle meshes with UV maps outside of 0-1 range
* Defining the atlas size could be somehow automatic
