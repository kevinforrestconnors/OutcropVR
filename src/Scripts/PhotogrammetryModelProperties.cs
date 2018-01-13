using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotogrammetryModelProperties : MonoBehaviour {

	private string modelName;

    private float UTMxCoordinate;
    private float UTMyCoordinate;

	public float meanX;
	public float meanY;
	public float minZ;

	public float xRange;
	public float yRange;
	public float zRange;

	public List<SDTPItem> items;

    public struct SDTPItem
    {
        private float x;
        private float y;
        private float z;
        private float strikeOrTrend;
        private float dipOrPlunge;
        private string type;

        public SDTPItem(float x, float y, float z, float st, float dp, string t)
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
			string sts = strikeOrTrend.ToString ();
			string dps = dipOrPlunge.ToString ();

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
			if (sts.IndexOf(".") > 0)
			{
				sts = sts.Substring(0, sts.IndexOf(".") + 2);
			}
			if (dps.IndexOf(".") > 0)
			{
				dps = dps.Substring(0, dps.IndexOf(".") + 2);
			}
			return xs + "," + ys + "," + zs + "," + sts + "," + dps + "," + type;
        }
	}

    public void AddSDTPItem(float x, float y, float z, float st, float dp, string t)
    {
		if (items == null) {
			items = new List<SDTPItem>();
		}
        items.Add(new SDTPItem(x, y, z, st, dp, t));
		WriteSDTPItemsToFile ();
    }

    public void WriteSDTPItemsToFile()
    {
        string[] lines = new string[items.Count + 3];

		lines [0] = "# Photogrammetry Model: " + modelName;
		lines [1] = "# Scale factor: " + meanX + " " + meanY + " " + minZ;
		lines [2] = " ";

        for (int i = 3; i < items.Count + 3; i++)
        {
            lines[i] = items[i - 3].ToString();
        }
		Debug.Log (lines[3]);
		File.WriteAllLines(Application.dataPath + "/SDTPData" + name + ".txt", lines);
    }

	public void SetName(string s) {
		modelName = s;
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

	public void SetScaleFactor(float x, float y, float z)
	{
		meanX = x;
		meanY = y;
		minZ = z;
	}

	public Vector3 GetScaleFactor()
	{
		return new Vector3(meanX, meanY, minZ);
	}
}
