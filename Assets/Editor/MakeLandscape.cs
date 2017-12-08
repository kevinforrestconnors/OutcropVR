//C# Example

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;

class MakeLandscape : EditorWindow {

	string minLong = "-79.65";
	string minLat = "37.6";
	string maxLong = "-79.35";
	string maxLat = "37.9";
	bool groupEnabled;
	float resolution = 90.0f;
	string modelName = "dem.obj";
	string mapName = "map.tiff";
	string done = "Generate";

	public string arguments() {
		return minLong + " " + minLat + " " + maxLong + " " + maxLat + " " + resolution.ToString("R") + " " + modelName + " " + mapName;
	}

	[MenuItem ("Tools/Generate Elevation Model")]
	public static void  ShowWindow () {
		EditorWindow.GetWindow(typeof(MakeLandscape));
	}

	void OnGUI () {
	
		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
		minLong = EditorGUILayout.TextField ("Min Longitude", minLong);
		minLat = EditorGUILayout.TextField ("Min Latitude", minLat);
		maxLong = EditorGUILayout.TextField ("Max Longitude", maxLong);
		maxLat = EditorGUILayout.TextField ("Max Longitude", maxLat);

		groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
		resolution = EditorGUILayout.Slider ("Resolution (m)", resolution, 30, 90);
		modelName = EditorGUILayout.TextField ("Model Filename", modelName);
		mapName = EditorGUILayout.TextField ("Map Filename", mapName);
		EditorGUILayout.EndToggleGroup ();
	
		if (GUILayout.Button (done)) {
			UnityEngine.Debug.Log("Generating...");

			Process p = new Process();

			string args = arguments ();
				
			p.StartInfo = new ProcessStartInfo(args) {
				FileName="/usr/local/Cellar/python3/3.6.3/Frameworks/Python.framework/Versions/3.6/bin/python3.6",
				Arguments = Application.dataPath+"/PythonScripts/objdem.py " + args,
				CreateNoWindow = true,
				UseShellExecute = true,
				RedirectStandardOutput = false,
				RedirectStandardInput = false,
				RedirectStandardError = false,
				WorkingDirectory = Application.dataPath
			};

			p.Start();
			p.WaitForExit();
			p.Close();

			UnityEngine.Debug.Log("Finished Generating.  Importing...");
		}
	}
}