using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;


class ConvertPhotogrammetryModel : EditorWindow
{
    public static string photogrammetryModelName = "photogrammetry.obj";
    public static string textureName = "photogrammetry_texture.jpg";
    string done = "Convert to Local Coordinates";

    public string arguments()
    {
        return "convertPhotogrammetry " + photogrammetryModelName;
    }

    [MenuItem("Tools/Localize Photogrammetry Model from UTM")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ConvertPhotogrammetryModel));
    }

    void OnGUI()
    {

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        photogrammetryModelName = EditorGUILayout.TextField("File Name: ", photogrammetryModelName);
        textureName = EditorGUILayout.TextField("Texture Name: ", textureName);

        if (GUILayout.Button(done))
        {

            UnityEngine.Debug.Log("Converting...");

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

            UnityEngine.Debug.Log("Finished Converting.  Importing...");

        }
    }
}
