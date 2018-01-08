using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//https://www.raywenderlich.com/149239/htc-vive-tutorial-unity
public class ControllerGrabObject : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;
    private GameObject collidingObject;
    private GameObject objectInHand;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    private void SetCollidingObject(Collider col)
    {
        if (collidingObject || !col.GetComponent<Rigidbody>())
        {
            return;
        }
        collidingObject = col.gameObject;
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered Collider");
        SetCollidingObject(other);
    }

    public void OnTriggerStay(Collider other)
    {
        if (objectInHand)
        {
            UpdatePosition();
            UpdateRotation();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (!collidingObject)
        {
            return;
        }

        collidingObject = null;
    }

    private void GrabObject()
    {
        Debug.Log("Grabbed Object");
        objectInHand = collidingObject;
        collidingObject = null;
    }

    private void ReleaseObject()
    {
        Debug.Log("Released object");
        objectInHand = null;
    }

    void Update () {

        if (Controller.GetHairTriggerDown())
        {
            if (collidingObject)
            {
                GrabObject();
            }
        }

        if (Controller.GetHairTriggerUp())
        {
            if (objectInHand)
            {
                ReleaseObject();
            }
        }
    }

    private void UpdatePosition()
    {
        objectInHand.transform.position = trackedObj.gameObject.transform.position;
    }

    private void UpdateRotation()
    {
        objectInHand.transform.Rotate(0, 0, 0);
        objectInHand.transform.Rotate(trackedObj.gameObject.transform.rotation.x, trackedObj.gameObject.transform.rotation.y, trackedObj.gameObject.transform.rotation.z);
    }
}
