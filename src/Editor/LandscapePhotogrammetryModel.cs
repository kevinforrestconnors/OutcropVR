using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;

class LandscapePhotogrammetryModel : EditorWindow
{

    public static string photogrammetryModelName = "photogrammetry.obj";
    public static string textureName = "photogrammetry_texture.jpg";
    string longRange = "0.05";
    string latRange = "0.05";
    bool groupEnabled;
    float resolution = 90.0f;
    string zone = "17N";
    public static string modelName = "photogrammetry_landscape.obj";
    public static string mapName = "photogrammetry_landscape_texture.tiff";
    string warning = "Use a coordinate range between 0.01 and 1.";
    string done = "Generate";

    [MenuItem("Tools/Make Elevation Model Around Photogrammetry Model")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(LandscapePhotogrammetryModel));
    }

    void OnGUI()
    {

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        photogrammetryModelName = EditorGUILayout.TextField("Photogrammetry Model: ", photogrammetryModelName);
        textureName = EditorGUILayout.TextField("Photogrammetry Texture: ", textureName);
        longRange = EditorGUILayout.TextField("Longitude Range", longRange);
        latRange = EditorGUILayout.TextField("Latitude Range", latRange);
        zone = EditorGUILayout.TextField("Zone", zone);

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
			objdem.LandscapePhotogrammetryModel (float.Parse (longRange), float.Parse (latRange), resolution, zone, photogrammetryModelName, modelName, mapName);

            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("Finished Generating.  Importing...");
        }
    }
}
