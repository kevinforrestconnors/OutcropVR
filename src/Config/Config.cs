using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour {

    // Edit these values to configure your project

	// add values to this list if you want to add a model that will be ignored by the left controller trigger toggle
	public static List<string> ignoredModels = new List<string> {"[SteamVR]", "Player", "EventSystem", "Directional Light", "VoiceManager", "Red Laser(Clone)", "Green Laser(Clone)", "Yellow Laser(Clone)", "OutcropVRLine(Clone)", "OutcropVRPlane(Clone)"};

    // change this to use a specified photogrammetry model for flying speed and line width calculations, or set it to "undefined" and an attempt will be made to locate the model
    public static string photogrammetryModelName = "undefined";

    // if true, uses Config.speed as the flying speed.  if false, uses calculated speed based off photogrammetry_modelScaled.obj
    public static bool useConfigSpeed = false;

    // if true, uses Config.lineWidth as the line width lines and for the height of planes.  if false, uses calculated size based off photogrammetry_modelScaled.obj
    public static bool useConfigLineWidth = false; 

    public static Speed speed = Speed.Medium;
    public static LineWidth lineWidth = LineWidth.Medium;

    // don't edit anything below this line unless you know what you're doing
    public enum Speed {
        Slow = 2,
        Medium = 10,
        Fast = 50,
        Lightning = 150,
		Infinity = 1000
    };

    public enum LineWidth
    {
        Small = 2,
        Medium = 10,
        Large = 50
    }
}
