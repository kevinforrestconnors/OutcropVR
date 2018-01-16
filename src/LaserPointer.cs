using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LaserPointer : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;

    private int frame; 

    public GameObject redLaserPrefab;
    public GameObject greenLaserPrefab;
    public GameObject yellowLaserPrefab;
	public GameObject outcropVRLinePrefab;
	public GameObject outcropVRPlanePrefab;

	private static GameObject currentGeoModel;

    private static GameObject redLaser;
    private static GameObject greenLaser;
    private static GameObject yellowLaser;
    private static GameObject laser;
    private Vector3 hitPoint;
	private RaycastHit lastHit;

    private enum Mode { LaserPointer, MakingLine, MakingPlane, MakingSurface, Paused, Saving, Deleting };
    private static Mode previousMode;
    private static Mode currentMode = Mode.LaserPointer;

    public static float lineWidth = 1;

    private List<Vector3> surfacePoints;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    private static Config.LineWidth CalculateLineWidthBasedOnModelSize()
    {
		float modelLength = Mathf.Sqrt (SDTP.xRange * SDTP.xRange + SDTP.yRange * SDTP.yRange + SDTP.zRange * SDTP.zRange);

		if (modelLength < 50)
        {
            return Config.LineWidth.Small;
        }
		else if (modelLength < 750)
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

		frame = 0;
		redLaser = Instantiate(redLaserPrefab);
		greenLaser = Instantiate(greenLaserPrefab);
		yellowLaser = Instantiate(yellowLaserPrefab);
		redLaser.SetActive(false);
		greenLaser.SetActive(false);
		yellowLaser.SetActive(false);
		laser = redLaser;
		surfacePoints = new List<Vector3>();

		// Set up SDTP
		SDTP.items = new List<SDTP.SDTPItem> ();
		SDTP.ReadSceneFileHeader ();

		if (!Config.useConfigLineWidth)
		{
			lineWidth = (int)CalculateLineWidthBasedOnModelSize() * 0.01f;
		}

		SDTP.RecoverSTDPItems ();
		DrawSDTPItems ();
    }

    void Start ()
    {

    }

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		SDTP.items = new List<SDTP.SDTPItem> ();
		SDTP.ReadSceneFileHeader ();
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
				lastHit = hit;

				if (hit.collider.gameObject.name == "OutcropVRPlane(Clone)" && currentMode == Mode.LaserPointer)
                {
                    GeoDisplay.SetActive();

					PlaneProperties p = hit.collider.gameObject.GetComponent<PlaneProperties>() as PlaneProperties;
                    GeoDisplay.UpdateStrikeAndDip(p.GetStrike(), p.GetDip());
                }
				else if (hit.collider.gameObject.name == "OutcropVRLine(Clone)" && currentMode == Mode.LaserPointer)
                {
                    GeoDisplay.SetActive();

					LineProperties l = hit.collider.gameObject.GetComponent<LineProperties>() as LineProperties;
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
		if (currentMode == Mode.LaserPointer) {
			laser.SetActive(false);
			currentMode = Mode.MakingLine;
			laser = greenLaser;
			GeoDisplay.SetInactive();
		}
    }

    public static void MakingPlane()
    {
		if (currentMode == Mode.LaserPointer) {
			laser.SetActive (false);
			currentMode = Mode.MakingPlane;
			laser = greenLaser;
			GeoDisplay.SetInactive ();
		}
    }

    public static void MakingSurface()
    {
		if (currentMode == Mode.LaserPointer) {
			laser.SetActive (false);
			currentMode = Mode.MakingSurface;
			laser = greenLaser;
			GeoDisplay.SetInactive ();
		}
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

	public static void Deleting() {
		if (currentMode == Mode.LaserPointer) {
			currentMode = Mode.Deleting;
		}
	}

    // Deletes a line, plane, or surface underneath the laser pointer
    public void Delete()
	{
		GameObject g = lastHit.collider.gameObject;

		if (g.name == "OutcropVRPlane(Clone)") {
			PlaneProperties pps = g.GetComponent<PlaneProperties>() as PlaneProperties;
			MeshFilter meshFilter = g.GetComponent<MeshFilter> () as MeshFilter;
			float hypot = Mathf.Abs (meshFilter.mesh.vertices [0].x) * 4;
			SDTP.DeleteSDTPItem(new SDTP.SDTPItem(g.transform.position.x, g.transform.position.y, g.transform.position.z, pps.GetStrike(), pps.GetDip(), hypot, "SD"));
			Destroy (g);
		} else if (g.name == "OutcropVRLine(Clone)")
		{
			LineProperties lps = g.GetComponent<LineProperties>() as LineProperties;
			float lineLength = g.transform.localScale.y * 2;
			SDTP.DeleteSDTPItem(new SDTP.SDTPItem(g.transform.position.x, g.transform.position.y, g.transform.position.z, lps.GetTrend(), lps.GetPlunge(), lineLength, "TP"));
			Destroy (g);
		}
    }

	public void CreateLine(Vector3 centroid, float trend, float plunge, float hypot) {

		GameObject trendPlungeObj = Instantiate (outcropVRLinePrefab);

		LineProperties lps = trendPlungeObj.AddComponent<LineProperties>() as LineProperties;
		lps.SetTrend (trend);
		lps.SetPlunge (plunge);

		Vector3 dir = Helpers.GetDirCosFromTrendAndPlunge (new Vector2 (trend, plunge)).normalized;

		Vector3 p1 = centroid - (dir * hypot * 0.5f);
		Vector3 p2 = centroid + (dir * hypot * 0.5f);

		// Find the length of the line and scale appropriately
		float lineLength = Vector3.Distance(p1, p2);
		float lineWidth = LaserPointer.lineWidth;
		trendPlungeObj.transform.localScale = new Vector3(lineWidth, lineLength / 2, lineWidth);

		// Set Position
		trendPlungeObj.transform.position = centroid;

		// Set Rotation
		trendPlungeObj.transform.transform.rotation = Quaternion.FromToRotation(Vector3.up, p2 - p1);
	}

	public void CreatePlane(Vector3 centroid, float strike, float dip, float hypot) {
	
		// Find corner points on plane
		float x1 = centroid.x - hypot / 4;
		float x2 = centroid.x + hypot / 4;
		float width = x2 - x1;

		float z1 = centroid.z - hypot / 2;
		float z2 = centroid.z + hypot / 2;
		float length = z2 - z1;

		// Make plane game object
		GameObject strikeDipObj = Instantiate (outcropVRPlanePrefab);

		// Set strike and dip for GeoDisplay
		PlaneProperties pps = strikeDipObj.AddComponent<PlaneProperties> () as PlaneProperties;
		pps.SetStrike (strike);
		pps.SetDip (dip);

		// Set position
		strikeDipObj.transform.position = centroid;

		// Set scale
		strikeDipObj.transform.localScale = new Vector3 (width, lineWidth, length);

		// Set rotation 
		if (strike < 180) {
			strikeDipObj.transform.Rotate (0, strike, -dip);
		} else {
			strikeDipObj.transform.Rotate (0, strike - 180, dip);
		}

	}

	public void DrawSDTPItems() {
	
		List<SDTP.SDTPItem> items = SDTP.items;

		foreach (SDTP.SDTPItem e in items) {

			if (e.type == "SD") {

				Vector3 centroid = new Vector3 (e.x, e.y, e.z);
				float strike = e.strikeOrTrend;
				float dip = e.dipOrPlunge;
				float hypot = e.hypot;

				CreatePlane (centroid, strike, dip, hypot);

			} else if (e.type == "TP") {
			
				Vector3 centroid = new Vector3 (e.x, e.y, e.z);
				float trend = e.strikeOrTrend;
				float plunge = e.dipOrPlunge;
				float hypot = e.hypot;

				CreateLine (centroid, trend, plunge, hypot);
		
			}
		}
	}

    // Update is called once per frame
    void Update () {

		if (currentMode == Mode.LaserPointer || currentMode == Mode.Paused) {
			if (Controller.GetHairTrigger ()) {
				DoLaser ();
			} else {
				laser.SetActive (false);
				GeoDisplay.SetInactive ();
			}
		} else if (currentMode == Mode.Deleting) {

			Delete ();
			currentMode = Mode.LaserPointer;

		} else
        {

			if (!currentGeoModel) {
				RaycastHit hit = DoLaser ();
				if (hit.collider.gameObject.name != "OutcropVRLine(Clone)" && 
					hit.collider.gameObject.name != "OutcropVRPlane(Clone)" && 
					hit.collider.gameObject.name != "Red Laser(Clone)" && 
					hit.collider.gameObject.name != "Green Laser(Clone)" && 
					hit.collider.gameObject.name != "Yellow Laser(Clone)") {
					currentGeoModel = hit.collider.transform.root.gameObject;
				}
			
			}

            if ((Controller.GetHairTrigger() || Controller.GetHairTriggerDown()) && currentMode != Mode.Paused)
            {
				
                RaycastHit hit = DoLaser();

                if (frame % 5 == 0 && hit.point != new Vector3(0.0f, 0.0f, 0.0f))
				{

					if (hit.collider.transform.root.gameObject.GetInstanceID () == currentGeoModel.GetInstanceID ()) {
						surfacePoints.Add (hit.point);
					}

                }

				Vector3[] points = new Vector3[surfacePoints.Count];

				for (int i = 0; i < surfacePoints.Count; i++) {
					points [i] = surfacePoints [i];
				}

				if (GameObject.Find("OutcropVRDrawLine"))
				{
					Destroy(GameObject.Find("OutcropVRDrawLine"));
				}

				LineRenderer drawLine = new GameObject("OutcropVRDrawLine").AddComponent(typeof(LineRenderer)) as LineRenderer;
				drawLine.material = new Material(Shader.Find("Unlit/Color"));
				drawLine.widthMultiplier = hit.distance * 0.01f;
				drawLine.numCornerVertices = 10;

				drawLine.positionCount = surfacePoints.Count;
				drawLine.SetPositions(points);

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

                    double lineX = result[0, 0];
					double lineY = result[1, 0];
                    double lineZ = result[2, 0];

					Vector3 dir = new Vector3((float)lineX, (float)lineY, (float)lineZ);
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

                    Vector2 trendAndPlunge = Helpers.GetTrendAndPlungeFromNormal(dir);
                    float trend = trendAndPlunge.x;
                    float plunge = trendAndPlunge.y;
				
					CreateLine (centroid, trend, plunge, hypot);

					// Write line to file
					SDTP.AddSDTPItem (centroid.x, centroid.y, centroid.z, trend, plunge, hypot, "TP");
					SDTP.WriteSDTPItemsToFile ();
                }
                else if (currentMode == Mode.MakingPlane)
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

					double normalX = result[0, 2];
					double normalY = result[1, 2];
					double normalZ = result[2, 2];

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

                    Vector2 strikeAndDip = Helpers.GetStrikeAndDipFromNormal(dir.normalized);

                    // Find strike and dip 
                    float strike = strikeAndDip.x;
                    float dip = strikeAndDip.y;

					CreatePlane (centroid, strike, dip, hypot);

					// Write plane to file
					SDTP.AddSDTPItem (centroid.x, centroid.y, centroid.z, strike, dip, hypot, "SD");
					SDTP.WriteSDTPItemsToFile ();
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
				currentGeoModel = null;
                laser.SetActive(false);
            }
        }

    }
}
