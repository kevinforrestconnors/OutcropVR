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
    string zoneNumber = "17";
    string zoneLetter = "N";
    public static string modelName = "photogrammetry_landscape.obj";
    public static string mapName = "photogrammetry_landscape_texture.tiff";
    string warning = "Use a coordinate range between 0.01 and 1.";
    string done = "Generate";

    public string arguments()
    {
        return "landscapePhotogrammetry " + longRange + " " + latRange + " " + resolution.ToString("R") + " " + zoneNumber + " " + zoneLetter + " " + photogrammetryModelName + " " + modelName + " " + mapName;
    }

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
        zoneNumber = EditorGUILayout.TextField("Zone Number", zoneNumber);
        zoneLetter = EditorGUILayout.TextField("Zone Letter", zoneLetter);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        resolution = EditorGUILayout.Slider("Resolution (m)", resolution, 30, 90);
        modelName = EditorGUILayout.TextField("Model Filename", modelName);
        mapName = EditorGUILayout.TextField("Map Filename", mapName);
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.LabelField(warning);


        if (GUILayout.Button(done))
        {

            UnityEngine.Debug.Log("Generating...");

            Process p = new Process();

            string args = arguments();

            p.StartInfo = new ProcessStartInfo(args)
            {
                FileName = "C:/Program Files/Python36/python.exe",
                Arguments = Application.dataPath + "/PythonScripts/objdem.py " + args,
                UseShellExecute = true,
                RedirectStandardOutput = false,
                RedirectStandardInput = false,
                RedirectStandardError = false,
                WorkingDirectory = Application.dataPath
            };

            p.Start();

            p.WaitForExit();
            p.Close();

            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("Finished Generating.  Importing...");
        }
    }
}
