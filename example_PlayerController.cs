using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour 
 {
     public float pos;
     private Rigidbody rb;
	private boatController boat;
	//private float c_mass = 100, c_u_kx = 0., c_u_ky, c_gravity;
	//public Vector3 v_position, v_speed, v_acceleration;
     void Start ()
     {
         rb = GetComponent<Rigidbody>();
		boat = new boatController (Time.fixedDeltaTime);
		pos = 0;
     }
     void FixedUpdate ()
     {
         float moveHorizontal = Input.GetAxis ("Horizontal");
         //float moveVertical = Input.GetAxis ("Vertical");
		if (moveHorizontal < 0) {
			boat.updateLinearVelocityWithForce (boat.c_forward_paddle_force);
			boat.updateAngularVelocityWithForceAndDistance (boat.c_forward_paddle_force, -0.9);

		} else if (moveHorizontal > 0){
			boat.updateLinearVelocityWithForce (boat.c_forward_paddle_force);
			boat.updateAngularVelocityWithForceAndDistance (boat.c_forward_paddle_force, 0.9);
		} else {
			boat.updateLinearVelocityWithForce (0);
			boat.updateAngularVelocityWithForceAndDistance (0, 0);
		}
		boat.updateHeadingAndPosition ();
		 //Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
         //rb.AddForce (movement * speed);
		//pos += moveHorizontal;
		//print("Heading %f, x %f, y %f", boat.heading, boat.position.x, boat.position.y);
		pos += 1;
		Vector3 p = new Vector3(boat.p_y, 0, boat.p_x);
		//print (boat.p_y);
		rb.MovePosition (p);
     }
	void OnTriggerEnter(Collider other) 
     {
         if (other.gameObject.CompareTag ("Pick Up"))
         {
             other.gameObject.SetActive (false);
         }
     }
 }
