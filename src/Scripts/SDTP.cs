﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public static class SDTP {

	public static string modelName;

	public static float meanX;
	public static float meanY;
	public static float minZ;

	public static float xRange;
	public static float yRange;
	public static float zRange;

	public static List<SDTPItem> items;

	public struct SDTPItem
	{
		public float x;
		public float y;
		public float z;
		public float hypot;
		public float strikeOrTrend;
		public float dipOrPlunge;
		public string type;

		public SDTPItem(float x, float y, float z, float st, float dp, float hypot, string t)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.strikeOrTrend = st;
			this.dipOrPlunge = dp;
			this.hypot = hypot; 
			this.type = t;
		}

		public bool Equals(SDTPItem other) {
			return Mathf.Abs(this.x - other.x) < 0.0001 &&
				Mathf.Abs(this.y - other.y) < 0.0001 &&
				Mathf.Abs(this.z - other.z) < 0.0001 &&
				Mathf.Abs(this.strikeOrTrend - other.strikeOrTrend) < 0.0001 &&
				Mathf.Abs(this.dipOrPlunge - other.dipOrPlunge) < 0.0001 &&
			this.type == other.type;
		}

		public override string ToString()
		{
			string xs = ((double)x + meanX).ToString ("0.000");
			string ys = ((double)y + meanY).ToString ("0.000");
			string zs = (z + minZ).ToString("0.00");
			string sts = strikeOrTrend.ToString ();
			string dps = dipOrPlunge.ToString ();
			string hols = hypot.ToString ();

			return xs + "," + ys + "," + zs + "," + sts + "," + dps + "," + hols + "," + type;
		}
	}

	public static void AddSDTPItem(float x, float y, float z, float st, float dp, float hypotOrLength, string t)
	{
		if (items == null) {
			items = new List<SDTPItem>();
		}

		items.Add(new SDTPItem(x, y, z, st, dp, hypotOrLength, t));
	}

	public static void DeleteSDTPItem(SDTPItem item) {

		for (int i = 0; i < items.Count; i++) {

			if (item.Equals (items [i])) {
				items.RemoveRange (i, 1);
				WriteSDTPItemsToFile ();
				return;
			}
		}
	}

	public static void WriteSDTPItemsToFile()
	{
		string[] lines;

		if (items != null) {
			lines = new string[items.Count + 4];	
			lines [0] = "# Photogrammetry Model: " + modelName;
			lines [1] = "# Shift: -" + meanX + " -" + meanY + " -" + minZ;
			lines [2] = "# Range: " + xRange + " " + yRange + " " + zRange;
			lines [3] = " ";

			for (int i = 4; i < items.Count + 4; i++)
			{
				lines[i] = items[i - 4].ToString();
			}

		} else {
			lines = new string[4];
			lines [0] = "# Photogrammetry Model: " + modelName;
			lines [1] = "# Shift: -" + meanX + " -" + meanY + " -" + minZ;
			lines [2] = "# Range: " + xRange + " " + yRange + " " + zRange;
			lines [3] = " ";
		}

		File.WriteAllLines(Application.dataPath + "/" + SceneManager.GetActiveScene ().name + "SDTPData.txt", lines);
	}

	public static bool SceneFileExists() {
		return ReadSceneFileHeader ();
	}

	public static bool ReadSceneFileHeader()
	{
		if (File.Exists(Application.dataPath + "/" + SceneManager.GetActiveScene ().name + "SDTPData.txt")) {
			StreamReader file = new StreamReader (Application.dataPath + "/" + SceneManager.GetActiveScene ().name + "SDTPData.txt");
			string line;

			while ((line = file.ReadLine ()) != null) {

				if (line.StartsWith ("# Photogrammetry Model: ")) {
					modelName = line.Substring ("# Photogrammetry Model: ".Length);
				}

				if (line.StartsWith ("# Shift: ")) {
					string[] scaleFactor = line.Substring ("# Shift: ".Length).Split (new char[] { ' ' });
					meanX = float.Parse(scaleFactor [0].Substring(1)); // ignore the negative sign
					meanY = float.Parse(scaleFactor [1].Substring(1));
					minZ = float.Parse(scaleFactor [2].Substring(1));
				}

				if (line.StartsWith ("# Range: ")) {
					string[] range = line.Substring("# Range: ".Length).Split (new char[] { ' ' });
					xRange = float.Parse(range [0]);
					yRange = float.Parse(range [1]);
					zRange = float.Parse(range [2]);
				}
			}

			file.Close ();
			return true;
		} else {
			return false;
		}
	}
		
	public static void RecoverSTDPItems() {

		if (File.Exists (Application.dataPath + "/" + SceneManager.GetActiveScene ().name + "SDTPData.txt")) {
			StreamReader file = new StreamReader (Application.dataPath + "/" + SceneManager.GetActiveScene ().name + "SDTPData.txt");
			string line;

			while ((line = file.ReadLine ()) != null) {

				if (line.EndsWith ("SD")) {
					string[] outcropVRPlane = line.Split (new char[] { ',' });
					double centroidX = double.Parse (outcropVRPlane [0]);
					double centroidY = double.Parse (outcropVRPlane [1]);
					double centroidZ = double.Parse (outcropVRPlane [2]);
					float strike = float.Parse (outcropVRPlane [3]);
					float dip = float.Parse (outcropVRPlane [4]);
					float hypot = float.Parse (outcropVRPlane [5]);

					// Add STDPItem with shift
					AddSDTPItem ((float)centroidX - meanX, (float)centroidY - meanY, (float)centroidZ - minZ, strike, dip, hypot, "SD");
				} else if (line.EndsWith ("TP")) {
					string[] outcropVRLine = line.Split (new char[] { ',' });
					double centroidX = double.Parse (outcropVRLine [0]);
					double centroidY = double.Parse (outcropVRLine [1]);
					double centroidZ = double.Parse (outcropVRLine [2]);
					float trend = float.Parse (outcropVRLine [3]);
					float plunge = float.Parse (outcropVRLine [4]);
					float hypot = float.Parse (outcropVRLine [5]);

					// Add STDPItem with shift
					AddSDTPItem ((float)centroidX - meanX, (float)centroidY - meanY, (float)centroidZ - minZ, trend, plunge, hypot, "TP");
				}
			}

			file.Close ();
		}

		WriteSDTPItemsToFile ();
	}
}

