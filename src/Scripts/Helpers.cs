using System;
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
            Debug.Log("SVDError: Need more rows than columns");
            return new double[0,0];
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
                    Debug.Log("SVDError: No convergence.");
                    return new double[0,0];
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

}
