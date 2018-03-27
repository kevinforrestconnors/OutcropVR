using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ObjDEM : ScriptableObject {

    private static string database = "https://data.worldwind.arc.nasa.gov";
    private static float meterPerDegreeLat = 111619.0f;
	private static float baseResolution = 30.0f;
	private static float resolutionInDeg = baseResolution / meterPerDegreeLat;

    private static List<int> elevationData = new List<int>();
    private static int[,] ElevationData;
	public static float meanX = 0.0f;
	public static float meanY = 0.0f;
	public static float minZ = 0.0f;
	public static float xRange = 0.0f;
	public static float yRange = 0.0f;
	public static float zRange = 0.0f;

    private IEnumerator WWWElevationData(string req, int width, int height)
    {
    // from https://forum.unity.com/threads/www-is-not-ready-downloading-yet.131989/
        WWW res = new WWW(req);
        yield return res;
        byte[] data = new byte[res.bytes.Length];
        Array.Copy(res.bytes, data, res.bytes.Length);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(data);
        }

		for (int i = data.Length - 1; i != -1; i -= 2)
        {
            short int16 = (short)(((data[i-1] & 0xFF) << 8) | (data[i] & 0xFF));
			elevationData.Add ((int)int16);
        }

        ElevationData = new int[height, width];

        for (int x = 0; x < height; x++)
        {
           for (int y = 0; y < width; y++)
            {
                int start = width * x;
                ElevationData[x, y] = elevationData[start + y];
            }
        }
    }
    
    // Fetches elevation data from data.worldwind.arc.nasa.gov
    private void FetchElevationData(float minLong, float maxLong, float minLat, float maxLat)
    {
        
        float longRange = maxLong - minLong;
        float latRange = maxLat - minLat;

        int width = (int)Mathf.Round(longRange / resolutionInDeg);
        int height = (int)Mathf.Round(latRange / resolutionInDeg);

        //print("    Querying database...");
        string req = database + "/elev?service=WMS&request=GetMap&layers=mergedSrtm&crs=EPSG:4326&format=image/bil&transparent=FALSE&width=" + width.ToString() + "&height=" + height.ToString() + "&bgcolor=0xFFFFFF&bbox=" + minLong.ToString() + "," + minLat.ToString() + "," + maxLong.ToString() + "," + maxLat.ToString() + "&styles=&version=1.3.0";

        //  from https://stackoverflow.com/questions/33251869/how-to-use-www-in-editor-script
        IEnumerator enumerator = WWWElevationData(req, width, height);

        // Current points to null here, so move it forward
        enumerator.MoveNext();

        // This blocks, but you can always use a thread
        while (!((WWW)(enumerator.Current)).isDone) ;

        // This triggers your 'Debug.Log(www.text)'
        enumerator.MoveNext();

        //print("    Fetched elevation data successfully.")
    
    }

	private void GetElevationDataFromRaw(string rawFile, int numRows, int numCols, bool use16Bit) {

		byte[] data;

		using (FileStream fs = new FileStream (Application.dataPath + "/" + rawFile, FileMode.Open, FileAccess.Read)) {

			data = new byte[fs.Length];

			int numBytesToRead = (int)fs.Length;
			int numBytesRead = 0;
			while (numBytesToRead > 0)
			{
				// Read may return anything from 0 to numBytesToRead.
				int n = fs.Read(data, numBytesRead, numBytesToRead);

				// Break when the end of the file is reached.
				if (n == 0)
					break;

				numBytesRead += n;
				numBytesToRead -= n;
			}
		} 

		if (BitConverter.IsLittleEndian) {
			Array.Reverse(data);
		}

		if (use16Bit) {
			for (int i = data.Length - 1; i > 0; i -= 2) {
				short int16 = (short)(((data [i - 1] & 0xFF) << 8) | (data [i] & 0xFF));
				elevationData.Add ((int)int16);
			}
		} else {
		
			for (int i = data.Length - 1; i != -1; i--) {
				elevationData.Add ((int)data [i]);
			}

		}

		ElevationData = new int[numRows, numCols];

		for (int x = 0; x < numRows; x++)
		{
			for (int y = 0; y < numCols; y++)
			{
				int start = numCols * x;
				ElevationData[x, y] = elevationData[start + y];
			}
		}
	}

    private IEnumerator WWWImageData(string req, string outfile)
    {
        WWW res = new WWW(req);
        yield return res;

        File.WriteAllBytes(Application.dataPath + "/" + outfile, res.bytes);
        AssetDatabase.Refresh();
    }

    // Fetches an image from data.worldwind.arc.nasa.gov
    private void FetchImageData(float minLong, float maxLong, float minLat, float maxLat, string filename)
    {

        if (minLat < -60.0 || maxLat > 84.0)
        {
            throw new Exception("FetchImageDataError: Landsat data is not available for values of latitude outside of the range -60.0 to 84.0");
        }

        float longRange = maxLong - minLong;
        float latRange = maxLat - minLat;

        int width = (int)Mathf.Round(longRange / resolutionInDeg);
        int height = (int)Mathf.Round(latRange / resolutionInDeg);

        string req = database + "/landsat?service=WMS&request=GetMap&layers=esat&crs=EPSG:4326&format=image/tiff&transparent=FALSE&width=" + width.ToString() + "&height=" + height.ToString() + "&bgcolor=0xFFFFFF&bbox=" + minLong.ToString() + "," + minLat.ToString() + "," + maxLong.ToString() + "," + maxLat.ToString() + "&styles=&version=1.3.0";

        IEnumerator enumerator = WWWImageData(req, filename);

        enumerator.MoveNext();

        // This blocks, but you can always use a thread
        while (!((WWW)(enumerator.Current)).isDone) ;

        enumerator.MoveNext();

    }
    
    // converts ElevationData to a Vector3[]
    private List<Vector3> ElevationPointsToXYZ(float minLong, float maxLong, float minLat, float maxLat)
    {

        List<Vector3> data = new List<Vector3>();

        for (int i = 0; i < ElevationData.GetLength(0); i++)
        {
            for (int j = 0; j < ElevationData.GetLength(1); j++) {
                double lon = minLong + resolutionInDeg * j;
                double lat = maxLat - resolutionInDeg * i;
                Vector2 UTM = Helpers.LonLatToUTM(lon, lat);
                float z = (float)ElevationData[i, j];
                Vector3 point = new Vector3(UTM.x, UTM.y, z);
                data.Add(point);
            }
        }

		if (SDTP.SceneFileExists ()) {
			xRange = SDTP.xRange;
			yRange = SDTP.yRange;
			zRange = SDTP.zRange;
			minZ = SDTP.minZ;
			meanX = SDTP.meanX;
			meanY = SDTP.meanY;
		} else {
			IEnumerable<float> xs = data.Select (e => e.x);
			IEnumerable<float> ys = data.Select (e => e.y);
			IEnumerable<float> zs = data.Select (e => e.z);

			float xMin = xs.Aggregate ((a, b) => a < b ? a : b);
			float yMin = ys.Aggregate ((a, b) => a < b ? a : b);
			float zMin = zs.Aggregate ((a, b) => a < b ? a : b);
			float xMax = xs.Aggregate ((a, b) => a > b ? a : b);
			float yMax = ys.Aggregate ((a, b) => a > b ? a : b);
			float zMax = zs.Aggregate ((a, b) => a > b ? a : b);

			// update global variables for later use
			xRange = xMax - xMin;
			yRange = yMax - yMin;
			zRange = zMax - zMin;
			minZ = zMin;
			meanX = (xMin + xMax) / 2;
			meanY = (yMin + yMax) / 2;
		}

        return data.Select(p => new Vector3(p.x - meanX, p.y - meanY, p.z - minZ)).ToList();
    }

	private List<Vector3> ElevationPointsToXYZ(float xOrigin, float yOrigin, float resolution) {

		List<Vector3> data = new List<Vector3>();

		for (int i = 0; i < ElevationData.GetLength(0); i++)
		{
			for (int j = 0; j < ElevationData.GetLength(1); j++) {
				float x = xOrigin + resolution * j;
				float y = yOrigin - resolution * i;
				float z = (float)ElevationData[i, j];
				Vector3 point = new Vector3(x, y, z);
				data.Add(point);
			}
		}

		if (SDTP.SceneFileExists ()) {
			xRange = SDTP.xRange;
			yRange = SDTP.yRange;
			zRange = SDTP.zRange;
			minZ = SDTP.minZ;
			meanX = SDTP.meanX;
			meanY = SDTP.meanY;
		} else {
			IEnumerable<float> xs = data.Select (e => e.x);
			IEnumerable<float> ys = data.Select (e => e.y);
			IEnumerable<float> zs = data.Select (e => e.z);

			float xMin = xs.Aggregate ((a, b) => a < b ? a : b);
			float yMin = ys.Aggregate ((a, b) => a < b ? a : b);
			float zMin = zs.Aggregate ((a, b) => a < b ? a : b);
			float xMax = xs.Aggregate ((a, b) => a > b ? a : b);
			float yMax = ys.Aggregate ((a, b) => a > b ? a : b);
			float zMax = zs.Aggregate ((a, b) => a > b ? a : b);

			// update global variables for later use
			xRange = xMax - xMin;
			yRange = yMax - yMin;
			zRange = zMax - zMin;
			minZ = zMin;
			meanX = (xMin + xMax) / 2;
			meanY = (yMin + yMax) / 2;
		}


		return data.Select(p => new Vector3(p.x - meanX, p.y - meanY, p.z - minZ)).ToList();
	
	}

    // Writes out points to an .obj file
    // Precondition: FetchElevationData has been called
    private void WritePointsToObj(List<Vector3> points, string filename)
    {
        string fp = Application.dataPath + "/" + filename;
        File.Delete(fp);
        File.Create(fp).Dispose();

		List<Vector3> triangles = Helpers.Triangulate(ElevationData.GetLength(1), ElevationData.GetLength(0));

        using (StreamWriter file = new StreamWriter(fp, true))
        {
           
			for (int i = 0; i < points.Count; i++)
			{
				Vector3 p = points[i];
				file.WriteLine("v " + p.x + " " + p.y + " " + p.z);
			}
				
            // write vertex textures for uv mapping
            int height = ElevationData.GetLength(0);
            int width = ElevationData.GetLength(1);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
					file.WriteLine("vt " + (j / (double)width).ToString() + " " + ((height - i) / (double)height).ToString() + " 0");
                }
                
            }

            // write facets
            // don't compute for 0,0 or width,height
            foreach (Vector3 simplex in triangles)
            {
                // cast to int since triangles are in ints, but Vector3 uses floats
                int x = (int)simplex.x;
                int y = (int)simplex.y;
                int z = (int)simplex.z;

                // we add 1 here because the .obj file indexing starts at 1, not 0.
                file.WriteLine("f " + (x + 1).ToString() + "/" + (x + 1).ToString() + " "
                                    + (y + 1).ToString() + "/" + (y + 1).ToString() + " "
                                    + (z + 1).ToString() + "/" + (z + 1).ToString());
            }
        }
        
    }
 
    // returns the long/lat of the centroid of the object
    public static Vector2 FindCentroidOfObj(string filename, string zone)
    {
		if (!filename.EndsWith(".obj")) throw new Exception("ObjDEMError: File must be of type .obj");

		string line;
		List<Vector3> xyzPoints = new List<Vector3>();

		StreamReader file = new StreamReader(Application.dataPath + "/" + filename);

		// read lines matching "v x y z" and turn that into a Vector3, which is put in xyzPoints
		while ((line = file.ReadLine()) != null) {

			if (line.StartsWith("v ")) {
				line = line.Substring (line.IndexOf ("v ") + 2);
				line = line.Trim();
				string[] point = line.Split(new char[] {' '});
				xyzPoints.Add(new Vector3(float.Parse(point[0]), float.Parse(point[1]), float.Parse(point[2])));
			}
		}

		file.Close ();

		IEnumerable<float> xs = xyzPoints.Select(e => e.x);
		IEnumerable<float> ys = xyzPoints.Select(e => e.y);

		float xMin = xs.Aggregate((a, b) => a < b ? a : b);
		float yMin = ys.Aggregate((a, b) => a < b ? a : b);
		float xMax = xs.Aggregate((a, b) => a > b ? a : b);
		float yMax = ys.Aggregate((a, b) => a > b ? a : b);

		float centerX = (xMax + xMin) / 2;
		float centerY = (yMax + yMin) / 2;

		return Helpers.UTMToLatLon (centerX, centerY, zone);
    }
		
    
    public static void ScaleDownObj(string filename)
    {

		if (!filename.EndsWith(".obj")) throw new Exception("ObjDEMError: File must be of type .obj");
			
		string line;
		List<double[]> xyzPoints = new List<double[]>();

		StreamReader file = new StreamReader(Application.dataPath + "/" + filename);

		// read lines matching "v x y z" and turn that into a Vector3, which is put in xyzPoints
		while ((line = file.ReadLine()) != null) {
		
			if (line.StartsWith("v ")) {
				line = line.Substring (line.IndexOf ("v ") + 2);
				line = line.Trim();
				string[] point = line.Split(new char[] {' '});
				xyzPoints.Add (new double[] { double.Parse (point [0]), double.Parse (point [1]), double.Parse (point [2]) });
			}
		}
	
		IEnumerable<double> xs = xyzPoints.Select(e => e[0]);
		IEnumerable<double> ys = xyzPoints.Select(e => e[1]);
		IEnumerable<double> zs = xyzPoints.Select(e => e[2]);
	
		double xMin = xs.Aggregate((a, b) => a < b ? a : b);
		double yMin = ys.Aggregate((a, b) => a < b ? a : b);
		double zMin = zs.Aggregate((a, b) => a < b ? a : b);
		double xMax = xs.Aggregate((a, b) => a > b ? a : b);
		double yMax = ys.Aggregate((a, b) => a > b ? a : b);
		double zMax = zs.Aggregate((a, b) => a > b ? a : b);

		// update global variables for later use
		xRange = (float)(xMax - xMin);
		yRange = (float)(yMax - yMin);
		zRange = (float)(zMax - zMin);
		minZ = (float)zMin;
		meanX = (float)(xMin + xMax) / 2;
		meanY = (float)(yMin + yMax) / 2;

	
		xyzPoints = xyzPoints.Select (p => new double[] { p [0] - meanX, p [1] - meanY, p [2] - minZ }).ToList ();

		// return to the top of the file
		file.DiscardBufferedData();
		file.BaseStream.Seek(0, SeekOrigin.Begin);

		// copy the file to <filename>Scaled.obj, this time replacing the "v x y z" lines with the new, scaled values
		string outfilename = filename.Substring(0, filename.IndexOf(".obj")) + "Scaled.obj";

		File.Delete(Application.dataPath + "/" + outfilename);
		File.Create(Application.dataPath + "/" + outfilename).Dispose();

		StreamWriter outfile = new StreamWriter (Application.dataPath + "/" + outfilename);

		int ln = 0;
		string l;

		while ((line = file.ReadLine()) != null) {

			if (line.StartsWith ("v ")) {
				l = "v " + xyzPoints [ln][0].ToString () + " " + xyzPoints [ln][1].ToString () + " " + xyzPoints [ln][2].ToString ();
				outfile.WriteLine (l);
				ln++;
			} else {
				outfile.WriteLine (line);
			}
		}

		file.Close ();
		outfile.Close ();

    }

	public static void CreateSceneFile(string name) {
		if (!SDTP.SceneFileExists ()) {
			SDTP.modelName = name;
			SDTP.meanX = meanX;
			SDTP.meanY = meanY;
			SDTP.minZ = minZ;
			SDTP.xRange = xRange;
			SDTP.yRange = yRange;
			SDTP.zRange = zRange;
			SDTP.WriteSDTPItemsToFile ();
		}
	}

    public void MakeLandscape(float minLong, float maxLong, float minLat, float maxLat, string modelFilename, string imageFilename)
    {
		elevationData = new List<int>();
		FetchImageData(minLong, maxLong, minLat, maxLat, imageFilename);
		FetchElevationData(minLong, maxLong, minLat, maxLat);
		List<Vector3> points = ElevationPointsToXYZ(minLong, maxLong, minLat, maxLat);
		WritePointsToObj(points, modelFilename);
		CreateSceneFile (modelFilename);
    }

	public void MakeLandScapeFromRAW(string rawFile, int numRows, int numCols, float xOrigin, float yOrigin, float resolution, bool use16Bit, string modelFilename) {
		elevationData = new List<int>();
		GetElevationDataFromRaw (rawFile, numRows, numCols, use16Bit);
		List<Vector3> points = ElevationPointsToXYZ (xOrigin, yOrigin, resolution);
		WritePointsToObj(points, modelFilename);
		CreateSceneFile (modelFilename);
	}

    public void ConvertPhotogrammetryModel(string photogrammetryFilename)
    {
        ScaleDownObj (photogrammetryFilename);
		CreateSceneFile (photogrammetryFilename);
    }


    public void LandscapePhotogrammetryModel(float longRange, float latRange, string zone, string photogrammetryFilename, string modelFilename, string imageFilename)
    {
		
        Vector2 centroid = FindCentroidOfObj(photogrammetryFilename, zone);
        float lon = centroid.x;
        float lat = centroid.y;
		float minLong = (float)(Math.Round ((double)(lon - longRange / 2), 2));
		float maxLong = (float)(Math.Round ((double)(lon + longRange / 2), 2));
		float minLat = (float)(Math.Round ((double)(lat - latRange / 2), 2));
		float maxLat = (float)(Math.Round ((double)(lat + latRange / 2), 2));

		elevationData = new List<int>();
        FetchElevationData(minLong, maxLong, minLat, maxLat);
        FetchImageData(minLong, maxLong, minLat, maxLat, imageFilename);
		List<Vector3> points = ElevationPointsToXYZ(minLong, maxLong, minLat, maxLat);
		WritePointsToObj (points, modelFilename);
		CreateSceneFile (photogrammetryFilename);
    }
}
