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
    private Vector3 initial_position = new Vector3((float)0.15, (float)0.5, 0);
    private Vector3 initial_rotation = new Vector3(0, 0, (float)(180.0 / 3.1416 * 90));
    
    void GrabObject()
    {
        grabbing = true;
        RaycastHit[] hits;
        if (!grabbedObject){
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
                grabbedObject.transform.parent = transform;
            }
        }
        else
        {
            Vector3 paddle_position = mainCamera.transform.position +
    initial_position;
            paddle_position.y = (float)0.5;
            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
            grabbedObject.GetComponent<Rigidbody>().position = paddle_position;
            Vector3 paddle_rotation = mainCamera.transform.rotation.eulerAngles + initial_rotation;
            grabbedObject.GetComponent<Rigidbody>().rotation = Quaternion.Euler(paddle_rotation);
        }


    }
    void DropObject()
    {
        grabbing = false;
        if (grabbedObject != null)
        {
            
            
            Vector3 paddle_position = mainCamera.transform.position +
                initial_position;
            paddle_position.y = (float)0.5;
            grabbedObject.GetComponent<Rigidbody>().position = paddle_position;
            Vector3 paddle_rotation = mainCamera.transform.rotation.eulerAngles + initial_rotation;
            grabbedObject.GetComponent<Rigidbody>().rotation = Quaternion.Euler(paddle_rotation);
            grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
            grabbedObject.transform.parent = null;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!grabbing && Input.GetAxis(buttonName) == 1)
        {
            GrabObject();
            GlobalVariables.grabbing_paddle = true;
        }
        if (grabbing && Input.GetAxis(buttonName) < 1)
        {
            DropObject();
            GlobalVariables.grabbing_paddle = false;
        }
    }


    // Use this for initialization
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        grabbing = false;
    }
}

public static class GlobalVariables
{
    public static bool grabbing_paddle;
}