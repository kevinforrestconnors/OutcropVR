using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegacyCode : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void MakePlaneFromNormalVector()
    {
        //Plane plane = new Plane(dir.normalized, centroid);

        //// plane will be made up of the closest points on the plane to these 4 points
        //Vector3 point1 = new Vector3(0, 0, 0);
        //Vector3 point2 = new Vector3(0, 0, 0);
        //Vector3 point3 = new Vector3(0, 0, 0);
        //Vector3 point4 = new Vector3(0, 0, 0);

        //if (shortestAxis == xRange)
        //{
        //    point1 = new Vector3(centroid.x, minY, minZ);
        //    point2 = new Vector3(centroid.x, minY, maxZ);
        //    point3 = new Vector3(centroid.x, maxY, minZ);
        //    point4 = new Vector3(centroid.x, maxY, maxZ);
        //}
        //else if (shortestAxis == yRange)
        //{
        //    point1 = new Vector3(minX, centroid.y, minZ);
        //    point2 = new Vector3(minX, centroid.y, maxZ);
        //    point3 = new Vector3(maxX, centroid.y, minZ);
        //    point4 = new Vector3(maxX, centroid.y, maxZ);
        //}
        //else if (shortestAxis == zRange)
        //{
        //    point1 = new Vector3(minX, minY, centroid.z);
        //    point2 = new Vector3(minX, maxY, centroid.z);
        //    point3 = new Vector3(maxX, minY, centroid.z);
        //    point4 = new Vector3(maxX, maxY, centroid.z);
        //}

        //// find closest points on the plane and center at 0,0,0
        //point1 = Helpers.ClosestPointOnPlane(plane, point1) - centroid;
        //point2 = Helpers.ClosestPointOnPlane(plane, point2) - centroid;
        //point3 = Helpers.ClosestPointOnPlane(plane, point3) - centroid;
        //point4 = Helpers.ClosestPointOnPlane(plane, point4) - centroid;

        //// Make mesh
        //Mesh m = new Mesh();
        //m.name = "Plane_Mesh";
        //m.vertices = new Vector3[] { point1, point2, point3, point4 };
        //m.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
        //m.triangles = new int[] { 0, 1, 2, 1, 2, 3 };
        //m.RecalculateNormals();

        //// Make plane game object
        //GameObject planeObj = new GameObject("Plane");
        //planeObj.AddComponent<BoxCollider>();
        //planeObj.transform.position = centroid;

        //planeObj.AddComponent<PlaneProperties>();
        //PlaneProperties planeProps = (PlaneProperties)planeObj.AddComponent<PlaneProperties>();
        //planeProps.SetStrike(strike);
        //planeProps.SetDip(dip);

        //// Make mesh
        //MeshFilter meshFilter = (MeshFilter)planeObj.AddComponent(typeof(MeshFilter));
        //meshFilter.mesh = m;
        //MeshRenderer renderer = planeObj.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        //renderer.material.shader = Shader.Find("Particles/Multiply");
        //Texture2D tex = new Texture2D(1, 1);
        //tex.SetPixel(0, 0, Color.green);
        //tex.Apply();
        //renderer.material.mainTexture = tex;
        //renderer.material.color = Color.green;
    }
}
