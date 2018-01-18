using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;

public class MakeLandscapeRAWFile : EditorWindow
{

	string RAWFile = "MouseRun.raw";
	string numRows = "376";
	string numCols = "300";
	string utmXOrigin = "636182";
	string utmYOrigin = "4190301";
	string resolution = "30";
	bool use16Bit = true;
	public static string modelName = "MouseRun.obj";
	public static string mapName = "MouseRun.tif";
	string done = "Generate";

	[MenuItem("Tools/Make Elevation Model from .raw")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(MakeLandscapeRAWFile));
	}

	void OnGUI()
	{

		GUILayout.Label("Base Settings", EditorStyles.boldLabel);
		RAWFile = EditorGUILayout.TextField("RAW File", RAWFile);
		numRows = EditorGUILayout.TextField("Number of Rows", numRows);
		numCols = EditorGUILayout.TextField("Number of Columns", numCols);
		utmXOrigin = EditorGUILayout.TextField("UTM Easting", utmXOrigin);
		utmYOrigin = EditorGUILayout.TextField("UTM Northing", utmYOrigin);
		resolution =  EditorGUILayout.TextField("Resolution", resolution);
		use16Bit = EditorGUILayout.Toggle ("Use 16 Bit (vs 8)", use16Bit);
		modelName = EditorGUILayout.TextField("Model Filename (output)", modelName);
		mapName = EditorGUILayout.TextField ("Map Name: (output)", mapName);

		if (GUILayout.Button(done))
		{

			UnityEngine.Debug.Log("Generating...");

			ObjDEM objdem = CreateInstance("ObjDEM") as ObjDEM;
			objdem.MakeLandScapeFromRAW (RAWFile, int.Parse(numRows), int.Parse(numCols), float.Parse(utmXOrigin), float.Parse(utmYOrigin), float.Parse(resolution), use16Bit, modelName);

			AssetDatabase.Refresh();

			UnityEngine.Debug.Log("Finished Generating.  Importing...");
		}
	}
}
