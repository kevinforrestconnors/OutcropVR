using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;

    private int frame; 

    public GameObject redLaserPrefab;
    public GameObject greenLaserPrefab;
    public GameObject yellowLaserPrefab;

    private static GameObject redLaser;
    private static GameObject greenLaser;
    private static GameObject yellowLaser;
    private static GameObject laser;
    private Vector3 hitPoint;

    private enum Mode { LaserPointer, MakingLine, MakingPlane, MakingSurface, Paused };
    private static Mode previousMode;
    private static Mode currentMode = Mode.LaserPointer;

    private static float lineWidth = 1;

    private List<Vector3> surfacePoints;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    private static Config.LineWidth CalculateLineWidthBasedOnModelSize()
    {
        GameObject g;

        if (Config.photogrammetryModelName.Equals("undefined"))
        {
            g = GameObject.FindWithTag("Photogrammetry Model");
        }
        else
        {
            g = GameObject.Find(Config.photogrammetryModelName);
        }

        Vector3 modelSize = new Vector3(0, 0, 0);

        Transform def = g.transform.GetChild(0);

        Transform hasChildren;
        try
        {
            hasChildren = def.GetChild(0);
        }
        catch
        {
            hasChildren = null;
        }

        if (!hasChildren)
        {
            modelSize = def.gameObject.GetComponent<Renderer>().bounds.size;
        }
        else
        {
            foreach (Transform meshPart in def)
            {
                modelSize += meshPart.gameObject.GetComponent<Renderer>().bounds.size;
            }
        }

        float modelVolume = modelSize.x * modelSize.y * modelSize.z;

        if (modelVolume < 5e6)
        {
            return Config.LineWidth.Small;
        }
        else if (modelVolume < 75e6)
        {
            return Config.LineWidth.Medium;
        }
        else
        {
            return Config.LineWidth.Large;
        }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start ()
    {
        frame = 0;
        redLaser = Instantiate(redLaserPrefab);
        greenLaser = Instantiate(greenLaserPrefab);
        yellowLaser = Instantiate(yellowLaserPrefab);
        redLaser.SetActive(false);
        greenLaser.SetActive(false);
        yellowLaser.SetActive(false);
        laser = redLaser;
        surfacePoints = new List<Vector3>();

        if (!Config.useConfigLineWidth)
        {
            
            lineWidth = (int)CalculateLineWidthBasedOnModelSize() * 0.01f;;
        }
    }

    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true);
        laser.transform.position = hitPoint;
        laser.transform.localScale = new Vector3((hit.distance * 0.01f), (hit.distance * 0.01f), (hit.distance * 0.01f));
    }

    private RaycastHit DoLaser()
    {
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 1000))
        {
            hitPoint = hit.point;
            
            if (hit.collider.gameObject.name != "Red Laser(Clone)" && hit.collider.gameObject.name != "Green Laser(Clone)" && hit.collider.gameObject.name != "Yellow Laser(Clone)")
            {
                ShowLaser(hit);

                if (hit.collider.gameObject.name == "OutcropVRPlane" && currentMode == Mode.LaserPointer)
                {
                    GeoDisplay.SetActive();

                    PlaneProperties p = hit.collider.gameObject.GetComponent<PlaneProperties>();
                    GeoDisplay.UpdateStrikeAndDip(p.GetStrike(), p.GetDip());
                }
                else if (hit.collider.gameObject.name == "OutcropVRLine" && currentMode == Mode.LaserPointer)
                {
                    GeoDisplay.SetActive();

                    LineProperties l = hit.collider.gameObject.GetComponent<LineProperties>();
                    GeoDisplay.UpdateTrendAndPlunge(l.GetTrend(), l.GetPlunge());
                }
                else
                {
                    GeoDisplay.SetInactive();
                }
            }
            else
            {
                laser.transform.localScale = new Vector3((hit.distance * 0.01f), (hit.distance * 0.01f), (hit.distance * 0.01f));
            }
        }

        return hit;
    }

    public static void MakingLine()
    {
        laser.SetActive(false);
        currentMode = Mode.MakingLine;
        laser = greenLaser;
        GeoDisplay.SetInactive();
    }

    public static void MakingPlane()
    {
        laser.SetActive(false);
        currentMode = Mode.MakingPlane;
        laser = greenLaser;
        GeoDisplay.SetInactive();
    }

    public static void MakingSurface()
    {
        laser.SetActive(false);
        currentMode = Mode.MakingSurface;
        laser = greenLaser;
        GeoDisplay.SetInactive();
    }

    public static void Pause()
    {
        if (currentMode != Mode.Paused)
        {
            previousMode = currentMode;
            currentMode = Mode.Paused;
            laser = yellowLaser;
        }
    }

    public static void Resume()
    {
        laser.SetActive(false);
        currentMode = previousMode;
        laser = greenLaser;
    }

    // Deletes a line, plane, or surface underneath the laser pointer
    public static void Delete()
    {

    }

    // Outputs a line, plane, or surface in the form X, Y, Z, Strike/Trend, Dip/Plunge, SD/TP, ID to Output Data/sdtpdata.txt
    public static void Save()
    {

    }

    // Update is called once per frame
    void Update () {

        if (currentMode == Mode.LaserPointer || currentMode == Mode.Paused)
        {
            if (Controller.GetHairTrigger())
            {
                DoLaser();
            }
            else
            {
                laser.SetActive(false);
                GeoDisplay.SetInactive();
            }
        }
        else
        {

            if ((Controller.GetHairTrigger() || Controller.GetHairTriggerDown()) && currentMode != Mode.Paused)
            {
                RaycastHit hit = DoLaser();

                if (frame % 5 == 0 && hit.point != new Vector3(0.0f, 0.0f, 0.0f))
                {
                    if (hit.collider.gameObject.name != "OutcropVRPlane" && hit.collider.gameObject.name != "OutcropVRLine")
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        surfacePoints.Add(hit.point);
                    }

                    if (GameObject.Find("OutcropVRDrawLine"))
                    {
                        Destroy(GameObject.Find("OutcropVRDrawLine"));
                    }

                    Vector3[] points = new Vector3[surfacePoints.Count];
                    for (int i = 0; i < surfacePoints.Count; i++)
                    {
                        points[i] = surfacePoints[i];
                    }

                    LineRenderer drawLine = new GameObject("OutcropVRDrawLine").AddComponent(typeof(LineRenderer)) as LineRenderer;
                    drawLine.material = new Material(Shader.Find("Unlit/Color"));
                    drawLine.widthMultiplier = hit.distance * 0.01f;
                    drawLine.numCornerVertices = 10;
                   
                    drawLine.positionCount = surfacePoints.Count;
                    drawLine.SetPositions(points);
                }
                frame++;
            }
            else if (Controller.GetHairTriggerUp() && currentMode != Mode.Paused)
            {

                laser.SetActive(false);

                // make the line, plane, or surface here
                if (currentMode == Mode.MakingLine)
                {

                    // arrange points in a matrix format
                    double[,] points = new double[surfacePoints.Count, 3];

                    int i = 0;
                    foreach (Vector3 point in surfacePoints)
                    {
                        points[i, 0] = point.x;
                        points[i, 1] = point.y;
                        points[i, 2] = point.z;
                        i++;
                    }

                    // from https://www.codefull.org/2015/06/3d-line-fitting/
                    // Find the mean of the points 
                    double meanX = 0.0;
                    double meanY = 0.0;
                    double meanZ = 0.0;
                    double[] means = new double[] { meanX, meanY, meanZ };

                    for (i = 0; i < points.GetLength(0); i++)
                    {
                        for (int j = 0; j < points.GetLength(1); j++)
                        {
                            means[j] += points[i, j];
                        }
                    }
                    means[0] /= points.GetLength(0);
                    means[1] /= points.GetLength(0);
                    means[2] /= points.GetLength(0);

                    // subtract the mean from all points
                    for (i = 0; i < points.GetLength(0); i++)
                    {
                        for (int j = 0; j < points.GetLength(1); j++)
                        {
                            points[i, j] -= means[j];
                        }
                    }

                    // peform SVD
                    double[,] result = Helpers.SVD_V(points);

                    double normalX = result[0, 0];
                    double normalY = result[1, 0];
                    double normalZ = result[2, 0];

                    Vector3 dir = new Vector3((float)normalX, (float)normalY, (float)normalZ);
                    Vector3 centroid = new Vector3((float)means[0], (float)means[1], (float)means[2]);

                    // Find xRange, yRange, zRange by finding min and max points
                    float minX = Mathf.Infinity;
                    float minY = Mathf.Infinity;
                    float minZ = Mathf.Infinity;

                    float maxX = -Mathf.Infinity;
                    float maxY = -Mathf.Infinity;
                    float maxZ = -Mathf.Infinity;

                    foreach (Vector3 point in surfacePoints)
                    {
                        // Find the minimum and maxiumum x, y, and z values
                        if (point.x < minX) { minX = point.x; }
                        if (point.y < minY) { minY = point.y; }
                        if (point.z < minZ) { minZ = point.z; }

                        if (point.x > maxX) { maxX = point.x; }
                        if (point.y > maxY) { maxY = point.y; }
                        if (point.z > maxZ) { maxZ = point.z; }
                    }

                    float xRange = maxX - minX;
                    float yRange = maxY - minY;
                    float zRange = maxZ - minZ;

                    float hypot = Mathf.Sqrt(xRange * xRange + yRange * yRange + zRange * zRange);

                    GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    line.name = "OutcropVRLine";

                    // Set trend and plunge properties for tooltip
                    Vector2 trendAndPlunge = Helpers.GetTrendAndPlungeFromNormal(dir);
                    float trend = trendAndPlunge.x;
                    float plunge = trendAndPlunge.y;
                    LineProperties lineProps = (LineProperties)line.AddComponent<LineProperties>();
                    lineProps.SetTrend(trend);
                    lineProps.SetPlunge(plunge);

                    // from https://forum.unity.com/threads/draw-cylinder-between-2-points.23510/ 

                    // Find the 2 points
                    //Vector3 hypotDir = dir * hypot;
                    Vector3 p1 = centroid - (dir * hypot * 0.5f);
                    Vector3 p2 = centroid + (dir * hypot * 0.5f);

                    // Find the length of the line and scale appropriately
                    float lineLength = Vector3.Distance(p1, p2) / 2;
                    line.transform.localScale = new Vector3(lineWidth, lineLength, lineWidth);

                    // Set Position
                    line.transform.position = centroid;

                    // Set Rotation
                    line.transform.transform.rotation = Quaternion.FromToRotation(Vector3.up, p2 - p1);

                }
                else if (currentMode == Mode.MakingPlane)
                {
                    // from https://www.ilikebigbits.com/blog/2015/3/2/plane-from-points
                    int n = surfacePoints.Count;

                    // three points are required
                    if (n < 3) { throw new Exception("Make Plane failed: three points are required"); }

                    Vector3 sum = new Vector3(0.0f, 0.0f, 0.0f);


                    float minX = Mathf.Infinity;
                    float minY = Mathf.Infinity;
                    float minZ = Mathf.Infinity;

                    float maxX = -Mathf.Infinity;
                    float maxY = -Mathf.Infinity;
                    float maxZ = -Mathf.Infinity;

                    foreach(Vector3 point in surfacePoints)
                    {
                        sum += point;

                        // Find the minimum and maxiumum x, y, and z values
                        if (point.x < minX) { minX = point.x; }
                        if (point.y < minY) { minY = point.y; }
                        if (point.z < minZ) { minZ = point.z; }

                        if (point.x > maxX) { maxX = point.x; }
                        if (point.y > maxY) { maxY = point.y; }
                        if (point.z > maxZ) { maxZ = point.z; }
                    }

                    float xRange = maxX - minX;
                    float yRange = maxY - minY;
                    float zRange = maxZ - minZ;

                    float hypot = Mathf.Sqrt(xRange * xRange + yRange * yRange + zRange * zRange);

                    // find shortest axis
                    // float secondShortestAxis = Mathf.Min(xRange, yRange);
                    // float shortestAxis = Mathf.Min(secondShortestAxis, zRange);

                    Vector3 centroid = sum / n;

                    // Calculate full 3x3 covariance matrix, excluding symmetries:
                    float xx = 0.0f;
                    float xy = 0.0f;
                    float xz = 0.0f;
                    float yy = 0.0f;
                    float yz = 0.0f;
                    float zz = 0.0f;

                    foreach(Vector3 point in surfacePoints)
                    {
                        Vector3 r = point - centroid;
                        xx += r.x * r.x;
                        xy += r.x * r.y;
                        xz += r.x * r.z;
                        yy += r.y * r.y;
                        yz += r.y * r.z;
                        zz += r.z * r.z;
                    }

                    float detX = yy * zz - yz * yz;
                    float detY = xx * zz - xz * xz;
                    float detZ = xx * yy - xy * xy;

                    // find max
                    float _maxXY = Mathf.Max(detX, detY);
                    float max = Mathf.Max(_maxXY, detZ);

                    if (max < 0) { throw new Exception("Make Plane failed: the points don't span a line"); }

                    Vector3 dir = new Vector3(0, 0, 0);

                    // pick path with best conditioning:
                    if (max == detX)
                    {
                        dir = new Vector3(detX, xz * yz - xy * zz, xy * yz - xz * yy);
                    } 
                    else if (max == detY)
                    {
                        dir = new Vector3(xz * yz - xy * zz, detY, xy * xz - yz * xx);
                    } else
                    {
                        dir = new Vector3(xy * yz - xz * yy, xy * xz - yz * xx, detZ);
                    } 

                    Vector2 strikeAndDip = Helpers.GetStrikeAndDipFromNormal(dir.normalized);

                    // Find strike and dip 
                    float strike = strikeAndDip.x;
                    float dip = strikeAndDip.y;

                    // Find corner points on plane
                    float x1 = centroid.x - hypot / 4;
                    float x2 = centroid.x + hypot / 4;
                    float width = x2 - x1;

                    float z1 = centroid.z - hypot / 2;
                    float z2 = centroid.z + hypot / 2;
                    float length = z2 - z1;

                    Vector3 p1 = new Vector3(x1, centroid.y, z1) - centroid;
                    Vector3 p2 = new Vector3(x1, centroid.y, z2) - centroid;
                    Vector3 p3 = new Vector3(x2, centroid.y, z1) - centroid;
                    Vector3 p4 = new Vector3(x2, centroid.y, z2) - centroid;

                    // Make mesh for plane
                    Mesh m = new Mesh();
                    m.name = "Plane_Mesh";
                    m.vertices = new Vector3[] { p1, p2, p3, p4 };
                    m.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
                    m.triangles = new int[] { 0, 1, 2, 1, 2, 3 };
                    m.RecalculateNormals();

                    // Make plane game object
                    GameObject strikeDipObj = new GameObject("OutcropVRPlane");

                    // Add a box collider so the plane can be collided with from both directions
                    BoxCollider bc = (BoxCollider)strikeDipObj.AddComponent<BoxCollider>();
                    bc.size = new Vector3(width, 1.0f , length);
                    bc.isTrigger = true;

                    // Add a rigid body so the plane can be grabbed and manipulated
                    // Rigidbody rb = (Rigidbody)strikeDipObj.AddComponent<Rigidbody>();
                    // rb.useGravity = false;
                    // rb.isKinematic = true;

                    // Set strike and dip for tooltip
                    PlaneProperties planeProps = (PlaneProperties)strikeDipObj.AddComponent<PlaneProperties>();
                    planeProps.SetStrike(strike);
                    planeProps.SetDip(dip);

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
                    strikeDipObj.transform.position = centroid;

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
        
                else if (currentMode == Mode.MakingSurface)
                {
                    Debug.Log("Currently, Make Surface is unimplemented");
                }

                laser = redLaser;
                currentMode = Mode.LaserPointer;
                surfacePoints = new List<Vector3>();
                if (GameObject.Find("OutcropVRDrawLine"))
                {
                    Destroy(GameObject.Find("OutcropVRDrawLine"));
                }
            }
            else
            {
                laser.SetActive(false);
            }
        }

    }
}
