using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotogrammetryModelProperties : MonoBehaviour {

    private float UTMxCoordinate;
    private float UTMyCoordinate;

    private float xRange;
    private float yRange;
    private float zRange;

    private struct SDTPItem
    {
        private float x;
        private float y;
        private float z;
        private string strikeOrTrend;
        private string dipOrPlunge;
        private string type;

        public SDTPItem(float x, float y, float z, string st, string dp, string t)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.strikeOrTrend = st;
            this.dipOrPlunge = dp;
            this.type = t;
        }
        
        public override string ToString()
        {
            // Truncate all but 1 sig fig
            string xs = x.ToString();
            string ys = y.ToString();
            string zs = z.ToString();

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
            return xs + "," + ys + "," + zs + "," + strikeOrTrend + "," + dipOrPlunge + "," + type;
        }
    }

    private List<SDTPItem> items;

    public void AddSDTPItem(float x, float y, float z, string st, string dp, string t)
    {
        items.Add(new SDTPItem(x, y, z, st, dp, t));
    }

    public void WriteSDTPItemsToFile()
    {
        string[] lines = new string[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            lines[i] = items[i].ToString();
        }
        System.IO.File.WriteAllLines(Application.dataPath + "/SDTPData.txt", lines);
    }

    public void SetRange(float x, float y, float z)
    {
        xRange = x;
        yRange = y;
        zRange = z;
    }

    public Vector3 GetRange()
    {
        return new Vector3(xRange, yRange, zRange);
    }

    // Use this for initialization
    void Start () {
        items = new List<SDTPItem>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
