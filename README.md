# elevation-unity

## Installation

1. `pip install numpy && pip install scipy && pip install utm`

2. Place `objdem.unitypackage` somewhere you can find it.  Open Unity and select `Assets/Import Package/Custom Package...`  

3. Find the line within `Editor/MakeLandscape.cs` that says `FileName="/usr/local/Cellar/python3/3.6.3/Frameworks/Python.framework/Versions/3.6/bin/python3.6"` and change the string to your python 3 path. Repeat for files `Editor/ConvertPhotogrammetryModel.cs` and `Editor/LandscapePhotogrammetryModel.cs`.

## Usage

### Tools/Generate Elevation Model

Use the Unity menu to select Tools/Generate Elevation Model.  This function produces a digital elevation map of an area specified with coordinates.  Note that too large of a range (more than a degree or so) or too small (a hundreth of a degree or so) will result in an error.

### Tools/Convert Photogrammetry Model

Use the Unity menu to select Tools/Convert Photogrammetry Model.
This function takes in a photogrammetry .obj file that is in UTM coordinates, and converts it into smaller numbers so that Unity can work with it.  If a texture file is supplied as well, it will be mapped onto the object. 

Supported texture formats: `.psd`, `.tiff`, `.jpg`, `.tga`, `.png`, `.gif`, `.bmp`, `.iff`, `.pict`.

### Tools/Landscape Photogrammetry Model

Use the Unity menu to select Tools/Landscape Photogrammetry Model.  This function takes in a photogrammetry .obj file that is in UTM coordinates, converts it to smaller numbers so that Unity can handle it, and models a landscape around the centroid of the photogrammetry model.  An unconverted UTM file is mandatory because finding the centroid requires the original georeferenced data.  If a texture file is supplied as well, it will be mapped onto the object. 

Supported texture formats: `.psd`, `.tiff`, `.jpg`, `.tga`, `.png`, `.gif`, `.bmp`, `.iff`, `.pict`.


## Troubleshooting

This is pretty difficult because Unity doesn't allow shell scripts to redirect standard output/error, so none of the error messages that `objdem.py` generate are shown.  You can try using https://github.com/kevinforrestconnors/objdem instead, which will produce the error messages.
