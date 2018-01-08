using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingMovement : MonoBehaviour {

    private GameObject player;
    private SteamVR_TrackedObject trackedObj;
    private static float speed = 50;
    private static float acceleratedSpeed = 0;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    public static void SpeedUp()
    {
        speed *= 2;
    }

    public static void SpeedDown()
    {
        speed /= 2;
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }
    

    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {

        float step = (speed + acceleratedSpeed) * Time.deltaTime;

        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {

            if (Controller.GetAxis().y > 0)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, player.transform.position + trackedObj.transform.forward, step);
            } else
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, player.transform.position - trackedObj.transform.forward, step);
            }
                
        } else
        {
            acceleratedSpeed = 0;
        }
    }
}
