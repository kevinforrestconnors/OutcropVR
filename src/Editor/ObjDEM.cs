using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class ObjDEM : MonoBehaviour {

    private static string database = "https://data.worldwind.arc.nasa.gov";
    private static float meterPerDegreeLat = 111619.0f;

    private static List<int> elevationData = new List<int>();
    private static float meanX = 0.0f;
    private static float meanY = 0.0f;
    private static float meanZ = 0.0f;

    private static IEnumerator DoWWW(string url)
    {
    // from https://forum.unity.com/threads/www-is-not-ready-downloading-yet.131989/
        WWW hs_post = new WWW(url);
        yield return hs_post;
    }

    public static void FetchElevationData(float minLong, float maxLong, float minLat, float maxLat, float resolution)
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
        StartCoroutine(DoWWW(req));




       Debug.Log(res.text);

        /*
        //print("    Converting data...");
        f = open('data.bil', 'wb')
    f.write(res.read())
    f.close()

    # Read from file
        b = array.array("h")
    with open("data.bil", "rb") as f:
        b.fromfile(f, width * height)
    if sys.byteorder == "big":
        b.byteswap()

    for x in range(0, height):
        row = []
        for y in range(0, width):
            start = width * x
            row.append(b[start + y])
        elevation_data.append(row)

    print("    Fetched elevation data successfully.")
    */
    }

    /*

def fetch_elevation_data(min_long, min_lat, max_long, max_lat, resolution):



# end function


def fetch_image_data(min_long, min_lat, max_long, max_lat, resolution=30, filename="landscape_texture.tiff"):

    if min_lat < -60.0 or max_lat > 84.0:
        print("Landsat data is not available for values of latitude outside of the range -60.0 to 84.0")
        # TODO find a database to serve this data
        return

    if (resolution < 30):
        resolution = 30

    resolution_in_deg = resolution / m_per_deg_lat

    long_range = max_long - min_long
    lat_range = max_lat - min_lat

    width = round(long_range / resolution_in_deg)
    height = round(lat_range / resolution_in_deg)

    print("    Querying database...")
    res = urlopen(worldwind +
                  '/landsat?'
                  'service=WMS'
                  '&request=GetMap'
                  '&layers=esat'
                  '&crs=EPSG:4326'
                  '&format=image/tiff'
                  '&transparent=FALSE'
                  '&width=' + str(width) +
                  '&height=' + str(height) +
                  '&bgcolor=0xFFFFFF'
                  '&bbox=' + str(min_long) + ',' + str(min_lat) + ',' + str(max_long) + ',' + str(max_lat) +
                  '&styles='
                  '&version=1.3.0')

    f = open(filename, 'wb')
    f.write(res.read())
    f.close()

    print("    Image created successfully.")


# end function

def elevation_points_to_xyz(min_long, min_lat, max_long, max_lat, resolution):
    global mean_x
    global mean_y
    global min_z

    resolution_in_deg = resolution / m_per_deg_lat

    data = []

    print("    Converting points to UTM...")
    for i in range(0, len(elevation_data)):
        for j in range(0, len(elevation_data[0])):
            long = min_long + resolution_in_deg * j
            lat = max_lat - resolution_in_deg * i
            (x, y, _zone_num, _zone_letter) = utm.from_latlon(lat, long)
            z = elevation_data[i][j]
            element = [round(x), round(y), z]
            data.append(element)

    xs = list(map(lambda e: e[0], data))
    ys = list(map(lambda e: e[1], data))
    zs = list(map(lambda e: e[2], data))

    mean_x = math.floor((min(xs) + max(xs)) / 2)
    mean_y = math.floor((min(ys) + max(ys)) / 2)
    min_z = math.floor(min(zs))

    data.append([min(xs) - 10, min(ys) - 10, min_z - 1])
    data.append([min(xs) - 10, max(ys) + 10, min_z - 1])
    data.append([max(xs) + 10, min(ys) - 10, min_z - 1])
    data.append([max(xs) + 10, max(ys) + 10, min_z - 1])

    return list(map(lambda e: [e[0] - mean_x, e[1] - mean_y, e[2] - min_z], data))
# end function


# precondition: fetch_elevation_data has been called
def write_points_to_obj(min_long, min_lat, max_long, max_lat, resolution, filename="landscape.obj"):
    try:
        os.remove(filename)
        f = open(filename, 'w')
    except FileNotFoundError:
        f = open(filename, 'w')

    points = elevation_points_to_xyz(min_long, min_lat, max_long, max_lat, resolution)

    print("    Writing points...")
    # write vertices
    for point in points:
        f.write("v " + str(point[0]) + " " + str(point[1]) + " " + str(point[2]) + '\n')

    # write vertex textures for uv mapping
    height = len(elevation_data)
    width = len(elevation_data[0])

    print("    UV mapping...")
    for i in range(0, height):
        for j in range(0, width):
            f.write("vt " + str(j / width) + " " + str(height - (i / height)) + ' 0\n')

    xy_points = numpy.array(list(map(lambda x: [x[0], x[1]], points)))

    tris = Delaunay(xy_points)

    a = len(points) - 1
    b = len(points) - 2
    c = len(points) - 3
    d = len(points) - 4

    print("    Writing triangles...")
    # write facets
    for simplex in tris.simplices:
        # don't compute for (0,0) or (width,height)
        if (simplex[0] != a and
                    simplex[1] != a and
                    simplex[2] != a and
                    simplex[0] != b and
                    simplex[1] != b and
                    simplex[2] != b and
                    simplex[0] != c and
                    simplex[1] != c and
                    simplex[2] != c and
                    simplex[0] != d and
                    simplex[1] != d and
                    simplex[2] != d):
            f.write("f " + str(simplex[0] + 1) + "/" + str(simplex[0] + 1) + " "
                    + str(simplex[1] + 1) + "/" + str(simplex[1] + 1) + " "
                    + str(simplex[2] + 1) + "/" + str(simplex[2] + 1) + '\n')

    f.close()
# end function


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


def main():

    if len(sys.argv) == 10 and sys.argv[1] == "landscapePhotogrammetry":

        print("Constructing a DEM around photogrammetry model...")

        long_range = float(sys.argv[2])
        lat_range = float(sys.argv[3])
        resolution = float(sys.argv[4])
        zone_number = float(sys.argv[5])
        zone_letter = sys.argv[6]
        photogrammetry_filename = sys.argv[7]
        model_filename = sys.argv[8]
        image_filename = sys.argv[9]

        print("    Finding centroid...")
        (centroidLong, centroidLat) = find_centroid_from_obj(photogrammetry_filename, zone_number, zone_letter)

        min_long = centroidLong - long_range / 2
        min_lat = centroidLat - lat_range / 2
        max_long = centroidLong + long_range / 2
        max_lat = centroidLat + lat_range / 2

    elif len(sys.argv) == 3 and sys.argv[1] == "convertPhotogrammetry":

        photogrammetry_filename = sys.argv[2]

        print("Scaling down photogrammetry model...")
        scale_down_obj(photogrammetry_filename, noModel=True)

        print("Successfully scaled down model.")
        time.sleep(3)

        return

    elif len(sys.argv) == 2 and sys.argv[1] == 'default':
        min_long = -79.65
        min_lat = 37.6
        max_long = -79.35
        max_lat = 37.9
        resolution = 90
        model_filename = "landscape.obj"
        image_filename = "landscape_texture.tiff"
    elif len(sys.argv) != 6 and len(sys.argv) != 8:
        print("Invalid number of arguments.  Usage: python objdem.py min_long min_lat max_long max_lat resolution")
        print("Or python objdem.py min_long min_lat max_long max_lat resolution model_filename image_filename")
        return
    else:
        min_long = float(sys.argv[1])
        min_lat = float(sys.argv[2])
        max_long = float(sys.argv[3])
        max_lat = float(sys.argv[4])
        resolution = float(sys.argv[5])

        if len(sys.argv) == 8:
            model_filename = sys.argv[6]
            image_filename = sys.argv[7]

    print("Fetching elevation data...")
    try:
        fetch_elevation_data(min_long, min_lat, max_long, max_lat, resolution)
    except HTTPError:
        print("    Coordinate range out of bounds.  Use a range between 0.01 and 1.")
        print("    Aborting...")
        time.sleep(3)

    os.remove("data.bil")

    print("Fetching image data...")
    try:
        fetch_image_data(min_long, min_lat, max_long, max_lat, 30, filename=image_filename)
    except HTTPError:
        print("    Coordinate range out of bounds.  Use a range between 0.01 and 1.")
        print("    Aborting...")
        time.sleep(3)

    print("Creating .obj file...")
    write_points_to_obj(min_long, min_lat, max_long, max_lat, resolution, filename=model_filename)
    print("Created .obj file successfully.")

    if len(sys.argv) == 10 and sys.argv[1] == "landscapePhotogrammetry":
        # this must be called after fetching elevation data so that it can be scaled based on the mean_x, mean_y, and min_z values
        print("Scaling photogrammetry .obj down based on elevation data...")
        scale_down_obj(photogrammetry_filename)

    time.sleep(3)
# end function


main()

     * 
     */

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
}
