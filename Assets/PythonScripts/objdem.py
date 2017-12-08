#!/usr/bin/env python

import sys, os, array, numpy, math, utm
from urllib.request import urlopen
from scipy.spatial import Delaunay

# the database
worldwind = 'https://data.worldwind.arc.nasa.gov'

elevation_data = []
m_per_deg_lat = 111619.0


def fetch_elevation_data(min_long, min_lat, max_long, max_lat, resolution):
    if (resolution < 30):
        resolution = 30

    resolution_in_deg = resolution / m_per_deg_lat

    long_range = max_long - min_long
    lat_range = max_lat - min_lat

    width = int(round(long_range / resolution_in_deg))
    height = int(round(lat_range / resolution_in_deg))

    res = urlopen(worldwind +
                 '/elev?'
                 'service=WMS'
                 '&request=GetMap'
                 '&layers=NED'
                 '&crs=EPSG:4326'
                 '&format=image/bil'
                 '&transparent=FALSE'
                 '&width=' + str(width) +
                 '&height=' + str(height) +
                 '&bgcolor=0xFFFFFF'
                 '&bbox=' + str(min_long) + ',' + str(min_lat) + ',' + str(max_long) + ',' + str(max_lat) +
                 '&styles='
                 '&version=1.3.0')

    f = open('data.bil', 'wb')
    f.write(res.read())
    f.close()

    # Read from file
    b = array.array("h")
    with open("data.bil", "rb") as f:
        b.fromfile(f, width * height)
    if sys.byteorder == "big":
        b.byteswap()

    for x in range(0, width):
        row = []
        for y in range(0, height):
            start = height * x
            row.append(b[start + y])
        elevation_data.append(row)


# end function


def fetch_image_data(min_long, min_lat, max_long, max_lat, resolution):
    if (resolution < 30):
        resolution = 30

    if min_lat < -60.0 or max_lat > 84.0:
        print("Landsat data is not available for values of latitude outside of the range -60.0 to 84.0")
        # TODO find a database to serve this data
        return

    resolution_in_deg = resolution / m_per_deg_lat

    long_range = max_long - min_long
    lat_range = max_lat - min_lat

    width = round(long_range / resolution_in_deg)
    height = round(lat_range / resolution_in_deg)

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

    f = open('map.tiff', 'wb')
    f.write(res.read())
    f.close()


def elevation_points_to_xyz(min_long, min_lat, max_long, max_lat, resolution):
    resolution_in_deg = resolution / m_per_deg_lat

    data = []

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
def write_points_to_obj(min_long, min_lat, max_long, max_lat, resolution):

    try:
        os.remove("dem.obj")
        f = open("dem.obj", 'a')
    except FileNotFoundError:
        f = open("dem.obj", 'a')

    points = elevation_points_to_xyz(min_long, min_lat, max_long, max_lat, resolution)

    # write vertices
    for point in points:
        f.write("v " + str(point[0]) + " " + str(point[1]) + " " + str(point[2]) + '\n')

    # writes vertex textures for uv mapping
    height = len(elevation_data)
    width = len(elevation_data[0])

    for i in range(0, height):
        for j in range(0, width):
            f.write("vt " + str(j / width) + " " + str(height - (i / height)) + ' 0\n')

    xy_points = numpy.array(list(map(lambda x: [x[0], x[1]], points)))

    tris = Delaunay(xy_points)

    a = len(points) - 1
    b = len(points) - 2
    c = len(points) - 3
    d = len(points) - 4

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

def main():
    if len(sys.argv) == 2 and sys.argv[1] == 'default':
        min_long = -79.65
        min_lat = 37.6
        max_long = -79.35
        max_lat = 37.9
        resolution = 90
    elif len(sys.argv) != 6:
        print("Invalid number of arguments.  Usage: python objDEM.py min_long min_lat max_long max_lat resolution")
        return
    else:
        min_long = float(sys.argv[1])
        min_lat = float(sys.argv[2])
        max_long = float(sys.argv[3])
        max_lat = float(sys.argv[4])
        resolution = float(sys.argv[5])

    fetch_elevation_data(min_long, min_lat, max_long, max_lat, resolution)
    fetch_image_data(min_long, min_lat, max_long, max_lat, 30)
    write_points_to_obj(min_long, min_lat, max_long, max_lat, resolution)

    os.remove("data.bil")


main()