using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helpers : MonoBehaviour {

    /* Finds a point on Plane p closest to Vector3 point */
    public static Vector3 ClosestPointOnPlane(Plane p, Vector3 point)
    {
        float d = Vector3.Dot(p.normal, point) + p.distance;
        return point - p.normal * d;
    }

    public static Vector2 UTMToLatLon(double utmX, double utmY, string utmZone)
    {
        // from https://stackoverflow.com/questions/2689836/converting-utm-wsg84-coordinates-to-latitude-and-longitude
        bool isNorthHemisphere = utmZone.Last() >= 'N';

        var diflat = -0.00066286966871111111111111111111111111;
        var diflon = -0.0003868060578;

        var zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
        var c_sa = 6378137.000000;
        var c_sb = 6356752.314245;
        var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
        var e2cuadrada = Math.Pow(e2, 2);
        var c = Math.Pow(c_sa, 2) / c_sb;
        var x = utmX - 500000;
        var y = isNorthHemisphere ? utmY : utmY - 10000000;

        var s = ((zone * 6.0) - 183.0);
        var lat = y / (c_sa * 0.9996);
        var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
        var a = x / v;
        var a1 = Math.Sin(2 * lat);
        var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
        var j2 = lat + (a1 / 2.0);
        var j4 = ((3 * j2) + a2) / 4.0;
        var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
        var alfa = (3.0 / 4.0) * e2cuadrada;
        var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
        var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
        var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
        var b = (y - bm) / v;
        var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
        var eps = a * (1 - (epsi / 3.0));
        var nab = (b * (1 - epsi)) + lat;
        var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
        var delt = Math.Atan(senoheps / (Math.Cos(nab)));
        var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

        double longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
        double latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;

        return new Vector2((float)longitude, (float)latitude);
    } 

    public static Vector2 LonLatToUTM(double lon, double lat)
    {
        // from https://stackoverflow.com/questions/176137/java-convert-lat-lon-to-utm
        double easting;
        double northing;
        int zone;

        zone = (int)Math.Floor(lon / 6 + 31);

        easting = 0.5 * Math.Log((1 + Math.Cos(lat * Math.PI / 180) * Math.Sin(lon * Math.PI / 180 - (6 * zone - 183) * Math.PI / 180)) / (1 - Math.Cos(lat * Math.PI / 180) * Math.Sin(lon * Math.PI / 180 - (6 * zone - 183) * Math.PI / 180))) * 0.9996 * 6399593.62 / Math.Pow((1 + Math.Pow(0.0820944379, 2) * Math.Pow(Math.Cos(lat * Math.PI / 180), 2)), 0.5) * (1 + Math.Pow(0.0820944379, 2) / 2 * Math.Pow((0.5 * Math.Log((1 + Math.Cos(lat * Math.PI / 180) * Math.Sin(lon * Math.PI / 180 - (6 * zone - 183) * Math.PI / 180)) / (1 - Math.Cos(lat * Math.PI / 180) * Math.Sin(lon * Math.PI / 180 - (6 * zone - 183) * Math.PI / 180)))), 2) * Math.Pow(Math.Cos(lat * Math.PI / 180), 2) / 3) + 500000;
        easting = Math.Round(easting * 100) * 0.01;
        northing = (Math.Atan(Math.Tan(lat * Math.PI / 180) / Math.Cos((lon * Math.PI / 180 - (6 * zone - 183) * Math.PI / 180))) - lat * Math.PI / 180) * 0.9996 * 6399593.625 / Math.Sqrt(1 + 0.006739496742 * Math.Pow(Math.Cos(lat * Math.PI / 180), 2)) * (1 + 0.006739496742 / 2 * Math.Pow(0.5 * Math.Log((1 + Math.Cos(lat * Math.PI / 180) * Math.Sin((lon * Math.PI / 180 - (6 * zone - 183) * Math.PI / 180))) / (1 - Math.Cos(lat * Math.PI / 180) * Math.Sin((lon * Math.PI / 180 - (6 * zone - 183) * Math.PI / 180)))), 2) * Math.Pow(Math.Cos(lat * Math.PI / 180), 2)) + 0.9996 * 6399593.625 * (lat * Math.PI / 180 - 0.005054622556 * (lat * Math.PI / 180 + Math.Sin(2 * lat * Math.PI / 180) / 2) + 4.258201531e-05 * (3 * (lat * Math.PI / 180 + Math.Sin(2 * lat * Math.PI / 180) / 2) + Math.Sin(2 * lat * Math.PI / 180) * Math.Pow(Math.Cos(lat * Math.PI / 180), 2)) / 4 - 1.674057895e-07 * (5 * (3 * (lat * Math.PI / 180 + Math.Sin(2 * lat * Math.PI / 180) / 2) + Math.Sin(2 * lat * Math.PI / 180) * Math.Pow(Math.Cos(lat * Math.PI / 180), 2)) / 4 + Math.Sin(2 * lat * Math.PI / 180) * Math.Pow(Math.Cos(lat * Math.PI / 180), 2) * Math.Pow(Math.Cos(lat * Math.PI / 180), 2)) / 3);
        if (lat < 0)
        northing = northing + 10000000;
        northing = Math.Round(northing * 100) * 0.01;

        return new Vector2((float)easting, (float)northing);
    }

    /* Transforms spherical to Cartesian coordinates 
     Preconditions: theta and phi must be in radians
         */
    public static Vector3 SphericalToCartesian(float theta, float phi, float radius)
    {
        float z = radius * Mathf.Sin(phi);
        float rCosElevation = radius * Mathf.Cos(phi);
        float x = rCosElevation * Mathf.Cos(theta);
        float y = rCosElevation * Mathf.Sin(theta);

        return new Vector3(x, y, z);
    }

    public static Vector3 SphericalToCartesian(Vector3 sphericalCoords)
    {
        return SphericalToCartesian(sphericalCoords.x, sphericalCoords.y, sphericalCoords.z);
    }

    public static Vector3 CartesianToSpherical(float x, float y, float z)
    {
        float hypotinuseXZ = Mathf.Sqrt(Mathf.Abs(x) * Mathf.Abs(x) + Mathf.Abs(z) * Mathf.Abs(z));
        float radius = Mathf.Sqrt(Mathf.Abs(hypotinuseXZ) * Mathf.Abs(hypotinuseXZ) + Mathf.Abs(y) * Mathf.Abs(y));
        float phi = Mathf.Atan2(y, hypotinuseXZ);
        float theta = Mathf.Atan2(z, x);

        return new Vector3(theta, phi, radius);
    }

    public static Vector3 CartesianToSpherical(Vector3 cartesianCoords)
    {
        return CartesianToSpherical(cartesianCoords.x, cartesianCoords.y, cartesianCoords.z);
    }

    public static Vector2 GetTrendAndPlungeFromNormal(Vector3 dir)
    {
        Vector3 spherical = CartesianToSpherical(dir);
        float azimuth = spherical.x;
        float elevation = spherical.y;
        // float radius = spherical.z;

        float trend;
        float plunge;

        if (elevation > 0)
        {
            trend = (3 * Mathf.PI / 2 - azimuth) * 180 / Mathf.PI;
            plunge = elevation * 180 / Mathf.PI;
        }
        else
        {
            trend = (Mathf.PI / 2 - azimuth) * 180 / Mathf.PI;
            plunge = -elevation * 180 / Mathf.PI;
        }

        if (trend > 360)
        {
            trend = trend - 360;
        }

        return new Vector2(trend, plunge);
    }

    public static Vector2 GetStrikeAndDipFromNormal(Vector3 dir)
    {
        Vector2 trendAndPlunge = GetTrendAndPlungeFromNormal(dir);
        float trend = trendAndPlunge.x;
        float plunge = trendAndPlunge.y;
        
        float strike;
        float dip;
        
        strike = trend + 90;
        if (strike > 360)
        {
            strike = strike - 360;
        } 

        dip = 90 - plunge;

        return new Vector2(strike, dip);
    }

    private static double svd_pythag(double a, double b)
    {
        double tolerance = 0.00000001;

        a = Math.Abs(a);
        b = Math.Abs(b);

        if (a > b)
        {
            return a * Math.Sqrt(1.0 + (b * b / a / a));
        }
        else if (b < tolerance)
        {
            return a;
        }

        return b * Math.Sqrt(1.0 + (a * a / b / b));
    }

    public static double[,] SVD_V(double[,] A)
    {
        // from http://www.numericjs.com/lib/numeric-1.2.6.js
        //Compute the thin SVD from G. H. Golub and C. Reinsch, Numer. Math. 14, 403-420 (1970)
        double temp;
        double prec = 2.220446049250313e-16; ; //Math.pow(2,-52) // assumes double prec
        double tolerance = (1e-64) / prec;
        double itmax = 50;
        double c = 0;
        int i = 0;
        int j = 0;
        int k = 0;
        int l = 0;

        double[,] u = new double[A.GetLength(0),A.GetLength(1)];
        for (i = 0; i < A.GetLength(0); i++)
        {
            for (j = 0; j < A.GetLength(1); j++)
            {
                u[i, j] = A[i, j];
            }
            
        }

        i = 0;
        j = 0;

        int m = u.GetLength(0);
        int n = u.GetLength(1);

        if (m < n) {
            throw new Exception("SVDError: Need more rows than columns");
        }

        double[] e = new double[n];
        double[] q = new double[n];
        double[,] v = new double[n,n];


        //Householder's reduction to bidiagonal form
        double f = 0.0;
        double g = 0.0;
        double h = 0.0;
        double x = 0.0;
        double y = 0.0;
        double z = 0.0;
        double s = 0.0;

        for (i = 0; i < n; i++) {
            e[i] = g;
            s = 0.0;
            l = i + 1;
            for (j = i; j < m; j++) {
                s += (u[j,i] * u[j,i]);
            }
            if (s <= tolerance){
                g = 0.0;
            }
            else
            {
                f = u[i, i];
                g = Math.Sqrt(s);
                if (f >= 0.0) {
                    g = -g;
                };
                h = f * g - s;
                u[i,i] = f - g;

                for (j = l; j < n; j++)
                {
                    s = 0.0;

                    for (k = i; k < m; k++) {
                        s += u[k,i] * u[k,j];
                    }

                    f = s / h;

                    for (k = i; k < m; k++){
                        u[k,j] += f * u[k,i];
                    }
                }   
            }
            q[i] = g;
            s = 0.0;
            for (j = l; j < n; j++){
                s = s + u[i,j] * u[i,j];
            }
            if (s <= tolerance) {
                g = 0.0;
            }
            else {
                f = u[i,i + 1];
                g = Math.Sqrt(s);
                if (f >= 0.0) {
                    g = -g;
                }
                h = f * g - s;
                u[i,i + 1] = f - g;
                for (j = l; j < n; j++) {
                    e[j] = u[i,j] / h;
                }
        
                for (j = l; j < m; j++) {
                    s = 0.0;
                    for (k = l; k < n; k++)
                    {
                        s += (u[j,k] * u[i,k]);
                    }
                    for (k = l; k < n; k++){
                        u[j,k] += s * e[k];
                    }
                }
            }

            y = Math.Abs(q[i]) + Math.Abs(e[i]);
            if (y > x) {
                x = y;
            }
        }

        // accumulation of right hand gtransformations
        for (i = n - 1; i != -1; i += -1) {
            if (g != 0.0) {
                h = g * u[i,i + 1];

                for (j = l; j < n; j++) {
                    v[j,i] = u[i,j] / h;
                }
                for (j = l; j < n; j++) {
                    s = 0.0;
                    for (k = l; k < n; k++) { 
                        s += u[i,k] * v[k,j];
                    }
                    for (k = l; k < n; k++) {
                        v[k,j] += (s * v[k,i]);
                    }
                }
            }

            for (j = l; j < n; j++)
            {
                v[i,j] = 0;
                v[j,i] = 0;
            }
            v[i,i] = 1;
            g = e[i];

            l = i;
        
        }

        // accumulation of left hand transformations
        for (i = n - 1; i != -1; i += -1) {
            l = i + 1;
            g = q[i];
        
            for (j = l; j < n; j++) {
                u[i,j] = 0;
            }
            if (g != 0.0)
            {
                h = u[i,i] * g;

                for (j = l; j < n; j++)
                {
                    s = 0.0;
                    for (k = l; k < m; k++) {
                        s += u[k,i] * u[k,j];
                    }
                    f = s / h;
                    for (k = i; k < m; k++) {
                        u[k,j] += f * u[k,i];
                    }
                }

                for (j = i; j < m; j++) {
                    u[j,i] = u[j,i] / g;
                }
            }
            else {
                for (j = i; j < m; j++) {
                    u[j,i] = 0;
                }
            }
            u[i,i] += 1;
        }

        // diagonalization of the bidiagonal form
        prec = prec * x;
 
        for (k = n - 1; k != -1; k += -1) {
            for (var iteration = 0; iteration < itmax; iteration++) {   // test f splitting
                bool test_convergence = false;
                for (l = k; l != -1; l += -1) {
                    if (Math.Abs(e[l]) <= prec) {
                        test_convergence = true;
                        break;
                    }
                    if (Math.Abs(q[l - 1]) <= prec) {
                        break;
                    }
                }
                if (!test_convergence)
                {   // cancellation of e[l] if l>0
                    c = 0.0;
                    s = 1.0;
                    int l1 = l - 1;

                    for (i = l; i < k + 1; i++)
                    {
                        f = s * e[i];
                        e[i] = c * e[i];
                        if (Math.Abs(f) <= prec) {
                            break;
                        }
                        g = q[i];
                        h = svd_pythag(f, g);
                        q[i] = h;
                        c = g / h;
                        s = -f / h;

                        for (j = 0; j < m; j++) {
                            y = u[j,l1];
                            z = u[j,i];
                            u[j,l1] = y * c + (z * s);
                            u[j,i] = -y * s + (z * c);
                        }
                    }
                }
                // test f convergence
                z = q[k];
                if (l == k)
                {   //convergence
                    if (z < 0.0)
                    {   //q[k] is made non-negative
                        q[k] = -z;
                        for (j = 0; j < n; j++) {
                            v[j,k] = -v[j,k];
                        }
                    }
                    break;  //break out of iteration loop and move on to next k value
                }
                if (iteration >= itmax - 1) {
                    throw new Exception("SVDError: No convergence.");
                }
                // shift from bottom 2x2 minor
                x = q[l];
                y = q[k - 1];
                g = e[k - 1];
                h = e[k];
                f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0 * h * y);
                g = svd_pythag(f, 1.0);
                if (f < 0.0)
                {
                    f = ((x - z) * (x + z) + h * (y / (f - g) - h)) / x;
                } else {
                    f = ((x - z) * (x + z) + h * (y / (f + g) - h)) / x;
                }
                    
                // next QR transformation
                c = 1.0;
                s = 1.0;
        
                for (i = l + 1; i < k + 1; i++) {
                    g = e[i];
                    y = q[i];
                    h = s * g;
                    g = c * g;
                    z = svd_pythag(f, h);
                    e[i - 1] = z;
                    c = f / z;
                    s = h / z;
                    f = x * c + g * s;
                    g = -x * s + g * c;
                    h = y * s;
                    y = y * c;

                    for (j = 0; j < n; j++) {
                        x = v[j,i - 1];
                        z = v[j,i];
                        v[j,i - 1] = x * c + z * s;
                        v[j,i] = -x * s + z * c;
                    }
                    z = svd_pythag(f, h);
                    q[i - 1] = z;
                    c = f / z;
                    s = h / z;
                    f = c * g + s * y;
                    x = -s * g + c * y;

                    for (j = 0; j < m; j++) {
                        y = u[j,i - 1];
                        z = u[j,i];
                        u[j,i - 1] = y * c + z * s;
                        u[j,i] = -y * s + z * c;
                    }
                }
                e[l] = 0.0;
                e[k] = f;
                q[k] = x;
        
            }
        }

        //vt= transpose(v)
        //return (u,q,vt)
        for (i = 0; i < q.Length; i++) {
            if (q[i] < prec) {
                q[i] = 0;
            }
        }
            
        //sort eigenvalues	
        for (i = 0; i < n; i++) {
            //writeln(q)
            for (j = i - 1; j >= 0; j--) {
                if (q[j] < q[i]) {
                    //  writeln(i,'-',j)
                    c = q[j];
                    q[j] = q[i];
                    q[i] = c;
              
                    for (k = 0; k < u.GetLength(0); k++) {
                        temp = u[k,i];
                        u[k,i] = u[k,j];
                        u[k,j] = temp;
                    }
                    for (k = 0; k < v.GetLength(0); k++) {
                        temp = v[k,i];
                        v[k,i] = v[k,j];
                        v[k,j] = temp;
                    }
                   //	   u.swapCols(i,j)
                   //	   v.swapCols(i,j)
                   i = j;
                }       
            }
        }

        return v;
   
    }

    private static float delaunayEpsilon = 1.0f / 1048576.0f;

    private static float[,] supertriangle(float[,] points)
    {
        float xmin = Mathf.Infinity, ymin = Mathf.Infinity,
        xmax = -Mathf.Infinity, ymax = -Mathf.Infinity;
        int i;
        float dx, dy, dmax, xmid, ymid;

        for (i = points.GetLength(0) - 1; i != -1;  i--)
        {
            if (points[i,0] < xmin) xmin = points[i,0];
            if (points[i,0] > xmax) xmax = points[i,0];
            if (points[i,1] < ymin) ymin = points[i,1];
            if (points[i,1] > ymax) ymax = points[i,1];
        }

        dx = xmax - xmin;
        dy = ymax - ymin;
        dmax = Mathf.Max(dx, dy);
        xmid = xmin + dx * 0.5f;
        ymid = ymin + dy * 0.5f;

        return new float[,] {
            { xmid - 20 * dmax, ymid - dmax },
            { xmid, ymid + 20 * dmax },
            { xmid + 20 * dmax, ymid - dmax }
        };           
    }

    private static Vector3[] circumcircle(float[,] points, int i, int j, int k)
    {
		
    float   x1 = points[i,0],
            y1 = points[i,1],
            x2 = points[j,0],
            y2 = points[j,1],
            x3 = points[k,0],
            y3 = points[k,1],
            fabsy1y2 = Mathf.Abs(y1 - y2),
            fabsy2y3 = Mathf.Abs(y2 - y3),
        xc, yc, m1, m2, mx1, mx2, my1, my2, dx, dy;
    if (fabsy1y2 < delaunayEpsilon && fabsy2y3 < delaunayEpsilon)
        {
            throw new Exception("DelaunayError: Coincident points");
        }
        

    if (fabsy1y2 < delaunayEpsilon)
    {
        m2 = -((x3 - x2) / (y3 - y2));
        mx2 = (x2 + x3) / 2.0f;
        my2 = (y2 + y3) / 2.0f;
        xc = (x2 + x1) / 2.0f;
        yc = m2 * (xc - mx2) + my2;
    }

    else if (fabsy2y3 < delaunayEpsilon)
    {
        m1 = -((x2 - x1) / (y2 - y1));
        mx1 = (x1 + x2) / 2.0f;
        my1 = (y1 + y2) / 2.0f;
        xc = (x3 + x2) / 2.0f;
        yc = m1 * (xc - mx1) + my1;
    }

    else
    {
        m1 = -((x2 - x1) / (y2 - y1));
        m2 = -((x3 - x2) / (y3 - y2));
        mx1 = (x1 + x2) / 2.0f;
        mx2 = (x2 + x3) / 2.0f;
        my1 = (y1 + y2) / 2.0f;
        my2 = (y2 + y3) / 2.0f;
        xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
        yc = (fabsy1y2 > fabsy2y3) ?
          m1 * (xc - mx1) + my1 :
          m2 * (xc - mx2) + my2;
    }

    dx = x2 - xc;
    dy = y2 - yc;
    return new Vector3[] { new Vector3(i, j, k), new Vector3(xc, yc, dx * dx + dy * dy) }; 
}
    
    private static List<int> dedup(List<int> edges)
    {
        int i, j;
        float a, b, m, n;

        for (j = edges.Count; j != 0; j += 0)
        {

			while (j > edges.Count) {
				j--;
			}

			b = edges[--j];
            a = edges[--j];

            for (i = j; i != 0; i += 0)
            {
                n = edges[--i];
                m = edges[--i];

                if ((a == m && b == n) || (a == n && b == m))
                {
					edges = edges.Take(j).Concat(edges.Skip(j + 2)).ToList ();
					edges = edges.Take(i).Concat(edges.Skip(i + 2)).ToList ();

                    break;
                }
            }
        }

        return edges;
    }

    private static int XPointsComparer(int i, int j, float[,] points)
    {
        float diff = points[j, 0] - points[i, 0];
        if (diff < 0)
        {
            return -1;
        } else if (diff > 0)
        {
            return 1;
        } else
        {
            return i - j;
        } 
    }

    public static List<Vector3> Delaunay(float[,] vertices)
    {   
        // Performs the Delaunay triangulation in 2 dimensions
        // Converted from https://github.com/ironwallaby/delaunay

        int n = vertices.GetLength(0),
        i, j, a, b, c;
        float dx, dy;


        /* Bail if there aren't enough vertices to form any triangles. */
        if (n < 3) { throw new Exception("DelaunayError: Not enough vertices to form any triangles."); }


        /* Slice out the actual vertices from the passed objects. (Duplicate the
            * array even if we don't, though, since we need to make a supertriangle
            * later on!) */
        
        float[,] points = vertices.Clone() as float[,];

        /* Make an array of indices into the vertex array, sorted by the
         * vertices' x-position. Force stable sorting by comparing indices if
         * the x-positions are equal. */
        List<int> indices = new List<int>();

        /* fill the array with 0..1..2..n */
        for (i = n - 1; i != -1; i--)
        {
			indices.Add(i);
        }

        // reverse the list before sorting
		indices.Reverse();

        indices.Sort((x, y) => XPointsComparer(x, y, points));

        /* Next, find the vertices of the supertriangle (which contains all other
         * triangles), and append them onto the end of a (copy of) the vertex
         * array. */
		float[,] pts = new float[points.GetLength(0) + 3, points.GetLength(1)];

		for (i = 0; i < points.GetLength (0); i++) {
			for (j = 0; j < points.GetLength (1); j++) {
				pts [i, j] = points [i, j];
			}
		}

        float[,] st = supertriangle(points);
		pts[points.GetLength(0), 0] = st[0, 0];
		pts[points.GetLength(0), 1] = st[0, 1];
		pts[points.GetLength(0) + 1, 0] = st[1, 0];
		pts[points.GetLength(0) + 1, 1] = st[1, 1];
		pts[points.GetLength(0) + 2, 0] = st[2, 0];
		pts[points.GetLength(0) + 2, 1] = st[2, 1];
        
        /* Initialize the open list (containing the supertriangle and nothing
         * else) and the closed list (which is empty since we havn't processed
         * any triangles yet). */
        //return new Vector3[] { new Vector3(i, j, k), new Vector3(xc, yc, dx * dx + dy * dy) };
        List<Vector3[]> open = new List<Vector3[]>();
        open.Add(circumcircle(pts, n, n + 1, n + 2));
        List<int[]> closed = new List<int[]>();
        List<int> edges = new List<int>();
        
        /* Incrementally add each vertex to the mesh. */
        for (i = indices.Count - 1; i != -1;  i--)
        {

            // empty edges array
            edges = new List<int>();

            c = indices[i];

            /* For each open triangle, check to see if the current point is
             * inside it's circumcircle. If it is, remove the triangle and add
             * it's edges to an edge list. */
            for (j = open.Count - 1; j != -1; j--)
            {
                /* If this point is to the right of this triangle's circumcircle,
                 * then this triangle should never get checked again. Remove it
                 * from the open list, add it to the closed list, and skip. */
                dx = pts[c,0] - open[j][1].x;
                if (dx > 0.0 && dx * dx > open[j][1].z)
                {
                    closed.Add(new int[] { (int)open[j][0].x, (int)open[j][0].y, (int)open[j][0].z });
					open = open.Take(j).Concat(open.Skip (j + 1)).ToList ();
                    continue;
                }

                /* If we're outside the circumcircle, skip this triangle. */
                dy = pts[c,1] - open[j][1].y;
                if (dx * dx + dy * dy - open[j][1].z > delaunayEpsilon)
                    continue;

                /* Remove the triangle and add it's edges to the edge list. */
                edges.Add((int)open[j][0].x);
                edges.Add((int)open[j][0].y);
                edges.Add((int)open[j][0].y);
                edges.Add((int)open[j][0].z);
                edges.Add((int)open[j][0].z);
                edges.Add((int)open[j][0].x);

				open = open.Take(j).Concat(open.Skip (j + 1)).ToList ();
            }

            /* Remove any doubled edges. */
            edges = dedup(edges);

            /* Add a new triangle for each edge. */
            for (j = edges.Count; j != 0; j += 0)
            {
                b = edges[--j];
                a = edges[--j];
				open.Add(circumcircle(pts, a, b, c));
            }

        }
        
        /* Copy any remaining open triangles to the closed list, and then
            * remove any triangles that share a vertex with the supertriangle,
            * building a list of triplets that represent triangles. */
        List<Vector3> nClosed = new List<Vector3>();
        for (i = open.Count - 1; i != -1; i--)
		{
            nClosed.Add(new Vector3(open[i][0].x, open[i][0].y, open[i][0].z));
        }
		for (i = closed.Count - 1; i != -1; i--) {
			nClosed.Add (new Vector3 (closed[i][0], closed[i][1], closed[i][2]));
		}

        List<Vector3> nOpen = new List<Vector3>();

		for (i = nClosed.Count - 1; i != -1; i--) {
			if (nClosed[i].x < n && nClosed[i].y < n && nClosed[i].z < n) {
				nOpen.Add(new Vector3(nClosed[i].x, nClosed[i].y, nClosed[i].z));
			}
		}
        /* Yay, we're done! */
        return nOpen;

    }
}
