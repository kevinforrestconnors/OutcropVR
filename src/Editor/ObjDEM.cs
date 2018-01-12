using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ObjDEM : ScriptableObject {

    private static string database = "https://data.worldwind.arc.nasa.gov";
    private static float meterPerDegreeLat = 111619.0f;

    private static List<int> elevationData = new List<int>();
    private static int[,] ElevationData;
    private static float meanX = 0.0f;
    private static float meanY = 0.0f;
    private static float meanZ = 0.0f;

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
    private void FetchElevationData(float minLong, float maxLong, float minLat, float maxLat, float resolution)
    {
        if (resolution < 30)
        {
            resolution = 30;
        }

        float resolutionInDeg = resolution / meterPerDegreeLat;
        
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

    private IEnumerator WWWImageData(string req, string outfile)
    {
        WWW res = new WWW(req);
        yield return res;

        File.WriteAllBytes(outfile, res.bytes);
        AssetDatabase.Refresh();
    }

    // Fetches an image from data.worldwind.arc.nasa.gov
    private void FetchImageData(float minLong, float maxLong, float minLat, float maxLat, float resolution, string filename)
    {

        if (minLat < -60.0 || maxLat > 84.0)
        {
            throw new Exception("FetchImageDataError: Landsat data is not available for values of latitude outside of the range -60.0 to 84.0");
        }

        if (resolution < 30)
        {
            resolution = 30;
        }

        float resolutionInDeg = resolution / meterPerDegreeLat;

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
    private List<Vector3> ElevationPointsToXYZ(float minLong, float maxLong, float minLat, float maxLat, float resolution)
    {
        float resolutionInDeg = resolution / meterPerDegreeLat;

        List<Vector3> data = new List<Vector3>();

        // print("    Converting points to UTM...")

        for (int i = 0; i < ElevationData.GetLength(0); i++)
        {
            for (int j = 0; j < ElevationData.GetLength(1); j++) {
                double lon = minLong + resolutionInDeg * j;
                double lat = minLat - resolutionInDeg * i;
                Vector2 UTM = Helpers.LonLatToUTM(lon, lat);
                float z = (float)ElevationData[i, j];
                Vector3 point = new Vector3(UTM.x, UTM.y, z);
                data.Add(point);
            }
        }

        IEnumerable<float> xs = data.Select(e => e.x);
        IEnumerable<float> ys = data.Select(e => e.y);
        IEnumerable<float> zs = data.Select(e => e.z);

        float xMin = xs.Aggregate((a, b) => a < b ? a : b);
        float yMin = ys.Aggregate((a, b) => a < b ? a : b);
		float zMin = Mathf.Floor(zs.Aggregate((a, b) => a < b ? a : b));

        float xMax = xs.Aggregate((a, b) => a > b ? a : b);
        float yMax = ys.Aggregate((a, b) => a > b ? a : b);

        meanX = Mathf.Floor((xMin + xMax) / 2);
        meanY = Mathf.Floor((yMin + yMax) / 2);

        return data.Select(p => new Vector3(p.x - meanX, p.y - meanY, p.z - zMin)).ToList();
    }

    // Writes out points to an .obj file
    // Precondition: FetchElevationData has been called
    private void WritePointsToObj(float minLong, float maxLong, float minLat, float maxLat, float resolution, string filename)
    {
        string fp = Application.dataPath + "/" + filename;
        File.Delete(fp);
        File.Create(fp).Dispose();

        List<Vector3> points = ElevationPointsToXYZ(minLong, maxLong, minLat, maxLat, resolution);

		float[,] xyPoints = new float[points.Count, 2];

		for (int i = 0; i < points.Count; i++)
		{
			Vector3 p = points[i];
			xyPoints[i, 0] = p.x;
			xyPoints[i, 1] = p.y;
		}

		List<Vector3> triangles = Helpers.Triangulate(ElevationData.GetLength(0), ElevationData.GetLength(1));

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
            
            int a = points.Count - 1;
            int b = points.Count - 2;
            int c = points.Count - 3;
            int d = points.Count - 4;

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
        return new Vector2(0, 0);
    }

    /*

def find_centroid_from_obj(file, zone_number, zone_letter):
    xyz = []

    with open(file) as f:
        lines = f.readlines()
        for line in lines:
            if line.find("v ") == 0:
                content = line.strip('\n').strip('v').strip().split()
                xyz.append(content)

    xs = list(map(lambda x: float(x[0]), xyz))
    ys = list(map(lambda x: float(x[1]), xyz))

    centerX = max(xs) - ((max(xs) - min(xs)) / 2)
    centerY = max(ys) - ((max(ys) - min(ys)) / 2)

    centerLat, centerLong = utm.to_latlon(centerX, centerY, zone_number, zone_letter)

    return (centerLong, centerLat)
# end function
*/
    
        public static void ScaleDownObj(string filename)
    {

    }
        /*

def scale_down_obj(file, noModel=False):

    global mean_x
    global mean_y
    global min_z

    try:
        i = file.index(".obj")
    except ValueError:
        print("Filename must end in .obj")
        return

    outfile = file[0:i]
    fo = open(outfile + "Scaled.obj", "w")

    xyz_points = []

    f = open(file, "r")
    lines = f.readlines()

    for line in lines:
        if line.find("v ") == 0:
            line = line.strip('\n').strip('v').strip().split()
            xyz_points.append([float(line[0]), float(line[1]), float(line[2])])

    if noModel:
        xs = list(map(lambda e: e[0], xyz_points))
        ys = list(map(lambda e: e[1], xyz_points))
        zs = list(map(lambda e: e[2], xyz_points))

        mean_x = math.floor((min(xs) + max(xs)) / 2)
        mean_y = math.floor((min(ys) + max(ys)) / 2)
        min_z = math.floor(min(zs))

    xyz_points = list(map(lambda e: [e[0] - mean_x, e[1] - mean_y, e[2] - min_z], xyz_points))

    f.seek(0)
    p = 0
    for line in lines:
        if line.find("v ") == 0:
            line = "v " + str(xyz_points[p][0]) + " " + str(xyz_points[p][1]) + " " + str(xyz_points[p][2]) + "\n"
            fo.write(line)
            p += 1
        else:
            fo.write(line)
# end function



        */

    public void MakeLandscape(float minLong, float maxLong, float minLat, float maxLat, float resolution, string modelFilename, string imageFilename)
    {
		FetchImageData(minLong, maxLong, minLat, maxLat, resolution, imageFilename);
		FetchElevationData(minLong, maxLong, minLat, maxLat, resolution);    
        WritePointsToObj(minLong, maxLong, minLat, maxLat, resolution, modelFilename);
    }

    public void ConvertPhotogrammetryModel(string photogrammetryFilename)
    {
        ScaleDownObj(photogrammetryFilename);
    }

    public void LandscapePhotogrammetryModel(float longRange, float latRange, float resolution, string zone, string photogrammetryFilename, string modelFilename, string imageFilename)
    {
        //print("    Finding centroid...")

        Vector2 centroid = FindCentroidOfObj(photogrammetryFilename, zone);
        float lon = centroid.x;
        float lat = centroid.y;
        float minLong = lon - longRange / 2;
        float maxLong = lon + longRange / 2;
        float minLat = lat - latRange / 2;
        float maxLat = lat + latRange / 2;

        FetchElevationData(minLong, maxLong, minLat, maxLat, resolution);
        FetchImageData(minLong, maxLong, minLat, maxLat, resolution, imageFilename);
        WritePointsToObj(minLong, maxLong, minLat, maxLat, resolution, modelFilename);
        ScaleDownObj(photogrammetryFilename);
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
}
