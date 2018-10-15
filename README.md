# OutcropVR

## Introduction

OutcropVR is a toolkit for geologists.  One aspect of OutcropVR is of building a scene to view in VR.  OutcropVR supports creation of real-world landscape data from coordinates or georeferenced models, as well as localization from UTM values. 


The other aspect is the "game" mode tools.  You can fly around your favorite outcrops and see unique angles that are impossible or difficult to get in real life.  With the laser pointer tool, you can create lines and planes that then display the trend/plunge or strike/dip.  

![A Model of San Francisco Generated Through OutcropVR](/master/outcropVR-sf.png?raw=true "San Francisco")

## Installation

1. Clone or download the repository.

2. Place `OutcropVR.unitypackage` somewhere you can find it.  Open Unity and select `Assets/Import Package/Custom Package...` and browse to import the filename `OutcropVR.unitypackage`.

3. Click the `Import` button.

4. If Steam VR asks for Recommended project settings, click `Accept All`.

## Creating a Scene

The general workflow is:

1. Go to the Template scene.  Copy all of the objects in the scene; these are: `[SteamVR]`, `Player`, `EventSystem`, `Directional Light`, and `VoiceManager`.
2. Create a new scene.  Delete `Main Camera` and `Directional Light`, then paste the objects you copied in step 1.
3. Import models and textures to Unity
4. Use the `Tools` menu to localize your models and create landscapes (if desired)
5. That's it!  Hit play and strap on your Vive gear.

### Tools/Localize Photogrammetry Model from UTM

Use the Unity menu to select `Tools/Localize Photogrammetry Model from UTM`.
This function takes in a photogrammetry .obj file that is in UTM coordinates, and converts it into smaller numbers so that Unity can work with it.  If a texture file is supplied as well, it will be mapped onto the object.

Supported texture formats: `.psd`, `.tiff`, `.jpg`, `.tga`, `.png`, `.gif`, `.bmp`, `.iff`, `.pict`.

### Tools/Make Elevation Model Around Photogrammetry Model

Use the Unity menu to select `Tools/Make Elevation Model Around Photogrammetry Model`.  This function takes in a photogrammetry .obj file that is in UTM coordinates and creates a digital elevation model around the centroid of the photogrammetry model.  An unconverted UTM file is mandatory because finding the centroid requires the original georeferenced data.

Supported texture formats: `.psd`, `.tiff`, `.jpg`, `.tga`, `.png`, `.gif`, `.bmp`, `.iff`, `.pict`.

### Tools/Make Elevation Model from Range

Use the Unity menu to select `Tools/Make Elevation Model from Range`.  This function produces a digital elevation model of an area specified with coordinates.  Note that too large of a range (more than a degree or so) or too small (a hundreth of a degree or so) will result in an error.

### Tools/Make Elevation Model from .raw

Use the Unity menu to select `Tools/Make Elevation Model from .raw`.  This function takes 16 or 8-bit elevation data (integers representing an altitude) as a RAW file, and produces a digital elevation model.  Parameters required are: the number of rows in the .raw data, the number of columns, the UTM Easting and Northing values for the origin, and the resolution (in meters).  You might be able to find these values in a `.hdr` file, depending on how you obtained the `.raw` data.

## In-Scene Functions

### Left Controller

Currently, the left controller does not have any functionality.

### Right Controller

* Trackpad Click (top half) - Fly forwards in the direction the controller is facing
* Trackpad Click (bottom half) - Fly backwards in the direction the controller is facing
* Trigger - Activates the laser pointer

### Voice Commands

* `Speed up` or `Increase speed`: Increases the speed at which you fly
* `Slow down` or `Decrease speed`: Decreases the speed at which you fly
* `Make line`: Activates the line-drawing function of the laser pointer. Hold down the trigger, say `Make line`, draw, and then release the trigger 
* `Make plane`: Activates the line-drawing function of the laser pointer. Hold down the trigger, say `Make plane`, draw, and then release the trigger
* `Make surface`: Unimplemented.
* `Pause` or `Pause drawing`: Pauses the drawing.  Hold down the trigger, say `Make line`, `Make plane`, or `Make surface` to begin drawing, then say `Pause`, then say `Resume`, then release the trigger to finish drawing.;
* `Resume` or `Resume drawing`: Resumes drawing.  See above.
* `Delete`: Deletes a line, plane or surface that is underneath an active laser pointer.
* `Save`: Outputs data about a line, plane, or surface that is underneath an active laser pointer, to a text file, in the format `X, Y, Z, Strike/Trend, Dip/Plunge, Type (SD or TP), ID`

## Troubleshooting Tools/

### I Can't See My Photogrammetry Model

It's likely not been scaled down to a size Unity can handle.  Use `Tools/Localize Photogrammetry Model from UTM` to do so.

### My Texture Looks Malformed

If you have two or more textures, this can happen.  Since we don't know which Meshparts have which texture, this can't be automated.  To fix this, manually inspect each Meshpart and under `Mesh Renderer/Materials`, change the size to allow for multiple textures, then put the materials in place.  I have found that the second texture should be the second element, etc.

## Troubleshooting OutcropVR

### `Make Plane`
Currently, `Make Plane` is not working as intended at certain angles. 

## Planned Features

* A compass that shows north, as a GUI item.
* Making surfaces with the laser pointer
* OpenVR support for multiple VR headsets (we only have the Vive to test on)
