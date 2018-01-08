using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineProperties : MonoBehaviour
{ 
    private float trend;
    private float plunge;

    public float GetTrend()
    {
        return trend;
    }

    public void SetTrend(float t)
    {
        trend = t;
    }

    public float GetPlunge()
    {
        return plunge;
    }

    public void SetPlunge(float p)
    {
        plunge = p;
    }
}
