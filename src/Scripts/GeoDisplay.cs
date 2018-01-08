using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeoDisplay : MonoBehaviour
{

    public Text strikeAndDip;

    private static string firstString = "Strike";
    private static string secondString = "Dip";
        
    private static float firstNumber = 0;
    private static float secondNumber = 0;

    private static bool panelActive = false;

    public static void SetActive()
    {
        panelActive = true;
    }

    public static void SetInactive()
    {
        panelActive = false;
    }

    public static void UpdateStrikeAndDip(float s, float d)
    {
        firstString = "Strike";
        secondString = "Dip";
        firstNumber = s;
        secondNumber = d;
    }

    public static void UpdateTrendAndPlunge(float t, float p)
    {
        firstString = "Trend";
        secondString = "Plunge";
        firstNumber = t;
        secondNumber = p;
    }

    private static string GetString()
    {
        if (!panelActive) { return ""; }

        string s = firstNumber.ToString();
        string d = secondNumber.ToString();

        // Truncate all but 1 sig fig
        if (s.IndexOf(".") > 0)
        {
            s = s.Substring(0, s.IndexOf(".") + 2);
        }

        if (d.IndexOf(".") > 0)
        {
            d = d.Substring(0, d.IndexOf(".") + 2);
        }

        return firstString + ": " + s + "\n" + secondString +  ": " + d;
    } 

    void Start()
    {
        strikeAndDip = GetComponent<Text>();
    }

    void Update()
    {
        strikeAndDip.text = GetString();
    }
}