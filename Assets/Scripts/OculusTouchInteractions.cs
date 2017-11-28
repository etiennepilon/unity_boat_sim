using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


public class OculusTouchInteractions : MonoBehaviour {
    private GameObject grabbedObject;
    private bool grabbing;
    public OVRInput.Controller controller;
    public string buttonName;
    public float grabRadius;
    public LayerMask grabMask;
    private GameObject paddle;
    private GameObject mainCamera;
    private Vector3 initial_position = new Vector3((float)0.15, (float)0.7, 0);
    private Vector3 initial_rotation = new Vector3(0, 0, (float)(180.0 / 3.1416 * 90));

    private GameObject hand_right, hand_left;
    
    void GrabObject()
    {
        grabbing = true;
        RaycastHit[] hits;

        hits = Physics.SphereCastAll(transform.position, grabRadius,
transform.forward, 0f, grabMask);
        if (hits.Length > 0)
        {
            int closestHit = 0;
            for (int i = 0; i < hits.Length; ++i)
            {
                if (hits[i].distance < hits[closestHit].distance) closestHit = i;
            }
            grabbedObject = hits[closestHit].transform.gameObject;
            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
            grabbedObject.transform.position = transform.position;
            grabbedObject.transform.eulerAngles = transform.eulerAngles;
            grabbedObject.transform.parent = transform;
            
            if (grabbedObject)
            {
                GlobalVariables.grabbing_paddle = true;
            }
        }
    }
    void DropObject()
    {
        grabbing = false;
        if (grabbedObject != null)
        {
            grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
            grabbedObject.transform.parent = null;
            GlobalVariables.grabbing_paddle = false;
            grabbedObject = null;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!grabbing && Input.GetAxis(buttonName) == 1)
        {
            GrabObject();
            
        }
        if (grabbing && Input.GetAxis(buttonName) < 1)
        {
            DropObject();

        }
    }


    // Use this for initialization
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        grabbing = false;
        hand_right = GameObject.FindGameObjectWithTag("right_hand");
        hand_left = GameObject.FindGameObjectWithTag("left_hand");
    }
}

public static class GlobalVariables
{
    public static bool grabbing_paddle;
} 
