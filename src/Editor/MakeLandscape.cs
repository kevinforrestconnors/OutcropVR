using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;

class MakeLandscape : EditorWindow
{

    string minLong = "-79.6";
    string maxLong = "-79.4";
    string minLat = "37.65";
    string maxLat = "37.85";
    bool groupEnabled;
    float resolution = 90.0f;
    public static string modelName = "landscape.obj";
    public static string mapName = "landscape_texture.tiff";
    string warning = "Use a coordinate range between 0.01 and 1.";
    string done = "Generate";

    [MenuItem("Tools/Make Elevation Model from Range")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MakeLandscape));
    }

    void OnGUI()
    {

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        minLong = EditorGUILayout.TextField("Min Longitude", minLong);
        maxLong = EditorGUILayout.TextField("Max Longitude", maxLong);
        minLat = EditorGUILayout.TextField("Min Latitude", minLat);
        maxLat = EditorGUILayout.TextField("Max Longitude", maxLat);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        resolution = EditorGUILayout.Slider("Resolution (m)", resolution, 30, 90);
        modelName = EditorGUILayout.TextField("Model Filename", modelName);
        mapName = EditorGUILayout.TextField("Map Filename", mapName);
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.LabelField(warning);

        if (GUILayout.Button(done))
        {

            UnityEngine.Debug.Log("Generating...");

            ObjDEM objdem = CreateInstance("ObjDEM") as ObjDEM;
			objdem.MakeLandscape (float.Parse(minLong), float.Parse(maxLong), float.Parse(minLat), float.Parse(maxLat), resolution, modelName, mapName);

			AssetDatabase.Refresh ();

            UnityEngine.Debug.Log("Finished Generating.  Importing...");
        }
    }
}
