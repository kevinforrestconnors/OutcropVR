# elevation-unity

## Installation

1. Place `objdem.unitypackage` somewhere you can find it.  Open Unity and select `Assets/Import Package/Custom Package...`  

2. Make sure `MakeLandscape.cs` is in a top-level folder called Editor.

3. Find the line within `MakeLandscape.cs` that says `FileName="/usr/local/Cellar/python3/3.6.3/Frameworks/Python.framework/Versions/3.6/bin/python3.6"` and change the string to your python 3 path. 

## Usage

Use the Unity menu to select Tools/Generate Elevation Model.  Then edit the settings to produce the desired model.  Note that too high of a range (more than a degree or so) or too low (a hundreth of a degree or so) will result in an error.

## Troubleshooting

This is pretty difficult because Unity doesn't allow shell scripts to redirect standard output/error, so none of the error messages that `objdem.py` generate are shown.  You can try using https://github.com/kevinforrestconnors/objdem instead, which will produce the error messages.
