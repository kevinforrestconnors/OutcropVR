# OutcropVR

## Installation
0. Install Python 3 and pip.

1. `pip install numpy && pip install scipy && pip install utm`

2. Place `OutcropVR.unitypackage` somewhere you can find it.  Open Unity and select `Assets/Import Package/Custom Package...` and browse to import the package.

3. Find the line within `Editor/MakeLandscape.cs` that says `FileName="C:/Program Files/Python36/python.exe"` and change the string to your python 3 path. Repeat for files `Editor/ConvertPhotogrammetryModel.cs` and `Editor/LandscapePhotogrammetryModel.cs`.

## Usage

### Tools/Make Elevation Model from Range

Use the Unity menu to select Tools/Make Elevation Model from Range.  This function produces a digital elevation map of an area specified with coordinates.  Note that too large of a range (more than a degree or so) or too small (a hundreth of a degree or so) will result in an error.

### Tools/Localize Photogrammetry Model from UTM

Use the Unity menu to select Tools/Localize Photogrammetry Model from UTM.
This function takes in a photogrammetry .obj file that is in UTM coordinates, and converts it into smaller numbers so that Unity can work with it.  If a texture file is supplied as well, it will be mapped onto the object. 

Supported texture formats: `.psd`, `.tiff`, `.jpg`, `.tga`, `.png`, `.gif`, `.bmp`, `.iff`, `.pict`.

### Tools/Make Elevation Model Around Photogrammetry Model

Use the Unity menu to select Tools/Make Elevation Model Around Photogrammetry Model.  This function takes in a photogrammetry .obj file that is in UTM coordinates, converts it to smaller numbers so that Unity can handle it, and models a landscape around the centroid of the photogrammetry model.  An unconverted UTM file is mandatory because finding the centroid requires the original georeferenced data.  If a texture file is supplied as well, it will be mapped onto the object. 

Supported texture formats: `.psd`, `.tiff`, `.jpg`, `.tga`, `.png`, `.gif`, `.bmp`, `.iff`, `.pict`.


## Troubleshooting Tools/

### My Texture Looks Malformmed

If you have two or more textures, this can happen.  Since we don't know which Meshparts have which texture, this can't be automated.  To fix this, manually inspect each Meshpart and under Mesh Renderer/Materials, change the size to allow for multiple textures, then put the materials in place.  I have found that the second texture should be the second element, etc.

## Troubleshooting OutcropVR

Do not name your GameObjects "Line", "Plane", or "Surface" because OutcropVR relies on matching these names for various functionality. 
