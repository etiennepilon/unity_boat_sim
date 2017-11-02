using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // Le code suivant sert a intercepter un ButtonDown event sur l’indexTrigger de
        // l’oculus Touch et change la couleur de l’objet
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            Color actualColor = this.gameObject.GetComponent<Renderer>().material.color;
            Color newColor = (actualColor == Color.gray ? Color.red : Color.grey);
            this.gameObject.GetComponent<Renderer>().material.color = newColor;
        }
    }
}
