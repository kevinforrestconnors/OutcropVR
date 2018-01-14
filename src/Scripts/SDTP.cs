using System;
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
		private float x;
		private float y;
		private float z;
		float hypot;
		private float strikeOrTrend;
		private float dipOrPlunge;
		private string type;

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
				Mathf.Abs(this.hypot - other.hypot) < 0.0001 &&
			this.type == other.type;
		}

		public override string ToString()
		{
			// Truncate all but 1 sig fig
			string xs = x.ToString();
			string ys = y.ToString();
			string zs = z.ToString();
			string sts = strikeOrTrend.ToString ();
			string dps = dipOrPlunge.ToString ();
			string hols = hypot.ToString ();

			if (xs.IndexOf(".") > 0)
			{
				xs = xs.Substring(0, xs.IndexOf(".") + 2);
			}
			if (ys.IndexOf(".") > 0)
			{
				ys = ys.Substring(0, ys.IndexOf(".") + 2);
			}
			if (zs.IndexOf(".") > 0)
			{
				zs = zs.Substring(0, zs.IndexOf(".") + 2);
			}
			if (sts.IndexOf(".") > 0)
			{
				sts = sts.Substring(0, sts.IndexOf(".") + 2);
			}
			if (dps.IndexOf(".") > 0)
			{
				dps = dps.Substring(0, dps.IndexOf(".") + 2);
			}
			if (hols.IndexOf(".") > 0)
			{
				hols = hols.Substring(0, hols.IndexOf(".") + 2);
			}
			return xs + "," + ys + "," + zs + "," + sts + "," + dps + "," + hols + "," + type;
		}
	}

	public static void AddSDTPItem(float x, float y, float z, float st, float dp, float hypotOrLength, string t)
	{
		if (items == null) {
			items = new List<SDTPItem>();
		}
		// we add meanX, meanY, and minZ because these are UTM values.  we're shifting back.
		items.Add(new SDTPItem(x + meanX, y + meanY, z + minZ, st, dp, hypotOrLength, t));
	}

	public static void DeleteSDTPItem(SDTPItem item) {

		for (int i = 0; i < items.Count; i++) {
		
			if (item.Equals(items[i])) {
				items.RemoveRange(i, 1);
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

		File.WriteAllLines(Application.dataPath + "/SDTPData" + SceneManager.GetActiveScene ().name + ".txt", lines);
	}

	public static void ReadSceneFileHeader()
	{
			
		StreamReader file = new StreamReader (Application.dataPath + "/SDTPData" + SceneManager.GetActiveScene ().name + ".txt");
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
	
	}
		
	public static void RecoverSTDPItems() {
		
		StreamReader file = new StreamReader(Application.dataPath + "/SDTPData" + SceneManager.GetActiveScene ().name + ".txt");
		string line;

		while ((line = file.ReadLine()) != null) {

			if (line.EndsWith ("SD")) {
				string[] outcropVRPlane = line.Split (new char[] {','});
				Vector3 centroid = new Vector3 (float.Parse (outcropVRPlane [0]), float.Parse (outcropVRPlane [1]), float.Parse (outcropVRPlane [2]));
				float strike = float.Parse (outcropVRPlane [3]);
				float dip = float.Parse (outcropVRPlane [4]);
				float hypot = float.Parse (outcropVRPlane [5]);

				AddSDTPItem (centroid.x - meanX, centroid.y - meanY, centroid.z - minZ, strike, dip, hypot, "SD");

				// The following code is nearly identical to the code in LaserPointer

				// Scale down
				Vector3 c = centroid - new Vector3 (meanX, meanY, minZ);

				// Find corner points on plane
				float x1 = c.x - hypot / 4;
				float x2 = c.x + hypot / 4;
				float width = x2 - x1;

				float z1 = c.z - hypot / 2;
				float z2 = c.z + hypot / 2;
				float length = z2 - z1;

				Vector3 p1 = new Vector3(x1, c.y, z1) - c;
				Vector3 p2 = new Vector3(x1, c.y, z2) - c;
				Vector3 p3 = new Vector3(x2, c.y, z1) - c;
				Vector3 p4 = new Vector3(x2, c.y, z2) - c;

				// Make mesh for plane
				Mesh m = new Mesh();
				m.name = "Plane_Mesh";
				m.vertices = new Vector3[] { p1, p2, p3, p4 };
				m.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
				m.triangles = new int[] { 0, 1, 2, 1, 2, 3 };
				m.RecalculateNormals();

				// Make plane game object
				GameObject strikeDipObj = new GameObject("OutcropVRPlane");

				PlaneProperties pps = strikeDipObj.AddComponent<PlaneProperties>() as PlaneProperties;
				pps.SetStrike (strike);
				pps.SetDip (dip);

				// Add a box collider so the plane can be collided with from both directions
				BoxCollider bc = (BoxCollider)strikeDipObj.AddComponent<BoxCollider>();
				bc.size = new Vector3(width, 1.0f , length);
				bc.isTrigger = true;
	
				// Make mesh
				MeshFilter meshFilter = (MeshFilter)strikeDipObj.AddComponent(typeof(MeshFilter));
				meshFilter.mesh = m;
				MeshRenderer renderer = strikeDipObj.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
				renderer.material.shader = Shader.Find("Particles/Multiply");
				Texture2D tex = new Texture2D(1, 1);
				tex.SetPixel(0, 0, new Color(0, 100, 100));
				tex.Apply();
				renderer.material.mainTexture = tex;
				renderer.material.color = new Color(0, 100, 100);

				// Set position
				strikeDipObj.transform.position = c;

				// Set Rotation 
				if (strike < 180)
				{
					strikeDipObj.transform.Rotate(0, strike, -dip);
				}
				else
				{
					strikeDipObj.transform.Rotate(0, strike - 180, dip);
				}

			} 
			else if (line.EndsWith ("TP")) {
				string[] outcropVRLine = line.Split (new char[] {','});
				Vector3 centroid = new Vector3 (float.Parse (outcropVRLine [0]), float.Parse (outcropVRLine [1]), float.Parse (outcropVRLine [2]));
				float trend = float.Parse (outcropVRLine [3]);
				float plunge = float.Parse (outcropVRLine [4]);
				float hypot = float.Parse (outcropVRLine [5]);

				// Add STDPItem with shift
				AddSDTPItem (centroid.x - meanX, centroid.y - meanY, centroid.z - minZ, trend, plunge, hypot, "SD");

				// The following code is nearly identical to the code in LaserPointer

				GameObject trendPlungeObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				trendPlungeObj.name = "OutcropVRLine";

				LineProperties lps = trendPlungeObj.AddComponent<LineProperties>() as LineProperties;
				lps.SetTrend (trend);
				lps.SetPlunge (plunge);

				Vector3 dir = Helpers.GetNormalFromTrendAndPlunge (new Vector2 (trend, plunge)).normalized;

				Vector3 p1 = centroid - (dir * hypot * 0.5f);
				Vector3 p2 = centroid + (dir * hypot * 0.5f);

				// Find the length of the line and scale appropriately
				float lineLength = Vector3.Distance(p1, p2) / 2;
				float lineWidth = LaserPointer.lineWidth;
				trendPlungeObj.transform.localScale = new Vector3(lineWidth, lineLength, lineWidth);

				// Set Position
				trendPlungeObj.transform.position = centroid;

				// Set Rotation
				trendPlungeObj.transform.transform.rotation = Quaternion.FromToRotation(Vector3.up, p2 - p1);

			}
		}

		file.Close ();
		WriteSDTPItemsToFile ();
	}
}

