using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneProperties : MonoBehaviour {

    private float strike;
    private float dip;

    public float GetStrike()
    {
        return strike;
    }

    public void SetStrike(float s)
    {
        strike = s;
    }

    public float GetDip()
    {
        return dip;
    }

    public void SetDip(float d)
    {
        dip = d;
    }
}
