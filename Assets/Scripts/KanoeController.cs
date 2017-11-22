using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KanoeController : MonoBehaviour {

    private Rigidbody rb;
    private boatCoordinateManager boat;
    private AudioSource audioSource;

    private bool controllingWithOneTouch = true;
    enum PaddlingState {NotPaddling, PaddlingLeft, PaddlingRight, BackPaddlingLeft, BackPaddlingRight};    // --
    // These values are to determine if the person is trying to circle
    private double incrementalTurn = 0;
    private bool goingLeft = false, goingRight = false;
    // --
    // All variables related to holding a Paddle
    private bool holdingPaddle = true;
    private double paddleAngleThreshold = 25.0 * 3.1416 / 180.0; // This is 10 degrees
    // --
    // This is the ugliest hack so far to avoid small sharp movements back and fo
    private int paddle_direction_buffer = 0, paddle_buffer_max = 20;

    // -- 
    // All Variables related to the One Touch Controller
    public OVRInput.Controller controller;
    private oneTouchCoordinates previousHandCoords, currentHandCoords;
    public struct oneTouchCoordinates
    {
        public float zleft, zright;
        public float dx, dy;
        public oneTouchCoordinates(float z1, float z2, float dX, float dY)
        {
            zleft = z1;
            zright = z2;
            dx = dX;
            dy = dY;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boat = new boatCoordinateManager(Time.fixedDeltaTime);
        currentHandCoords = new oneTouchCoordinates();
        previousHandCoords = new oneTouchCoordinates();

        audioSource = GetComponent<AudioSource>();
    }
    void FixedUpdate()
    {
        PaddlingState state = getPaddlingState();
        double paddleDistance = 0.9;
        //debug_state(state);
        if (state == PaddlingState.PaddlingLeft)
        {
            boat.paddleLeftWithCorrectionFactor(paddleDistance);
            audioSource.Play();
        }
        else if (state == PaddlingState.PaddlingRight)
        {
            boat.paddleRightWithCorrectionFactor(paddleDistance);
            audioSource.Play();
        }
        else if (state == PaddlingState.BackPaddlingLeft)
        {
            boat.backPaddleLeftWithPaddleDistance(paddleDistance);
            audioSource.Play();
        }
        else if (state == PaddlingState.BackPaddlingRight)
        {
            boat.backPaddleRightWithPaddleDistance(paddleDistance);
            audioSource.Play();
        }
        else boat.notPaddlingWithCorrection();

        rb.MovePosition(new Vector3(boat.p_y, 0, boat.p_x));
        Quaternion rotation = Quaternion.Euler(new Vector3(0, (float)(180.0 / 3.1416 * boat.heading), 0));
        rb.MoveRotation(rotation);
        if (GlobalVariables.grabbing_paddle)
        {
            Debug.Log("Grabbing paddle");
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up"))
        {
            other.gameObject.SetActive(false);
        }
    }
    // --
    // This Method determines the side the user is Paddling 
    private PaddlingState getPaddlingState()
    {
        PaddlingState state = PaddlingState.NotPaddling;
        if (controllingWithOneTouch && GlobalVariables.grabbing_paddle)
        {
            state = getOneTouchPaddlingState();
        }
        else if (controllingWithOneTouch && !GlobalVariables.grabbing_paddle)
        {
            // -- Should give a sign to the user to hold the paddle/close the hands
            state = PaddlingState.NotPaddling;
        }
        else
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            if (moveHorizontal < 0) state = PaddlingState.PaddlingLeft;
            else if (moveHorizontal > 0) state = PaddlingState.PaddlingRight;
        }
        boat.updateCorrectionFactor(correctionFactorForState(state));
        return state;
    }
    // --
    // This function returns the deltaX = Xright - Xleft and deltaY = Yright - Yleft
    //   in order to compute the angle. Its the equivalent as calculating the X, Y vector
    //   between both hands. 
    // It also returns zleft and zright. It will be required in order to know in which direction
    //   the paddle is going. If Zleft_previous - Zleft_new < 0, we know we're moving forward.
    // This assume local (relative to the user) coordinates.
    private void updateHandPositionVector()
    {
        Vector3 leftHandCoords = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        Vector3 rightHandCoords = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        currentHandCoords.zleft = leftHandCoords.z; currentHandCoords.zright = rightHandCoords.z;
        currentHandCoords.dx = rightHandCoords.x - leftHandCoords.x;
        currentHandCoords.dy = rightHandCoords.y - leftHandCoords.y;
    }
    private PaddlingState getOneTouchPaddlingState()
    {
        PaddlingState state = PaddlingState.NotPaddling;
        updateHandPositionVector();
        if (currentHandCoords.dx == 0.0) return state;
        // --
        // Calculate the angle between the right and left hand using the right hand as reference
        double theta = Math.Atan(currentHandCoords.dy / currentHandCoords.dx);
        //Debug.Log(theta );
        if (paddleIsInTheWater(theta) && paddleIsRightSide(theta))
        {
           if (paddleIsPushingWaterBack(previousHandCoords.zright - currentHandCoords.zright))
           {
                // - The buffer is positive when pushing water backward (so movement is forward)
                if (paddle_direction_buffer >= 0)
                {
                    state = PaddlingState.PaddlingRight;
                    if (paddle_direction_buffer < paddle_buffer_max) paddle_direction_buffer += 1;
                }
                else
                {
                    paddle_direction_buffer += 1;
                } 
           }
            else if (paddleIsPushingWaterForward(previousHandCoords.zright - currentHandCoords.zright))
            {
                if (paddle_direction_buffer < 0)
                {
                    state = PaddlingState.BackPaddlingRight;
                    if (paddle_direction_buffer > -paddle_buffer_max) paddle_direction_buffer -= 1;
                }
                else
                {
                    paddle_direction_buffer -= 1;
                }

            }
        }
        else if (paddleIsInTheWater(theta) && paddleIsLeftSide(theta))
        {
           if (paddleIsPushingWaterBack(previousHandCoords.zleft - currentHandCoords.zleft))
           {
                if (paddle_direction_buffer >= 0)
                {
                    state = PaddlingState.PaddlingLeft;
                    if (paddle_direction_buffer < paddle_buffer_max) paddle_direction_buffer += 1;
                }
                else
                {
                    paddle_direction_buffer += 1;
                }
            }
           else if (paddleIsPushingWaterForward(previousHandCoords.zleft - currentHandCoords.zleft))
            {
                if (paddle_direction_buffer < 0)
                {
                    state = PaddlingState.BackPaddlingLeft;
                    if (paddle_direction_buffer > -paddle_buffer_max) paddle_direction_buffer -= 1;
                }
                else
                {
                    paddle_direction_buffer -= 1;
                }
            }
        }
    
        previousHandCoords = currentHandCoords;
        return state;
    }
    private bool paddleIsInTheWater(double theta) { return Math.Abs(theta) > paddleAngleThreshold; }
    private bool paddleIsPushingWaterBack(double delta) { return delta > 0.003; }

    private bool paddleIsPushingWaterForward(double delta) { return delta < -0.003; }
    private bool paddleIsRightSide(double theta) { return (theta < 0);}
    private bool paddleIsLeftSide(double theta) { return (theta > 0); }
    // --
    // THis is a Hack - It adds a turning factor sa that it reduce the initial turn 
    private double correctionFactorForState(PaddlingState state)
    {
        double result = 0;
        if (state == PaddlingState.PaddlingLeft) result = Time.deltaTime;
        else if (state == PaddlingState.PaddlingRight) result = -Time.deltaTime;
       // else if (state == PaddlingState.BackPaddlingLeft) result = -Time.deltaTime;
       // else if (state == PaddlingState.BackPaddlingRight) result = Time.deltaTime;
        else if (state == PaddlingState.NotPaddling) result = 0;
        return result;
    }

    private void debug_state(PaddlingState state)
    {
        double theta = Math.Atan(currentHandCoords.dy / currentHandCoords.dx);
        switch (state)
        {
            case PaddlingState.PaddlingRight:
                Debug.Log("Paddling RIght");
                Debug.Log(paddle_direction_buffer);
                break;
            case PaddlingState.PaddlingLeft:
                Debug.Log("Paddling Left");
                Debug.Log(paddle_direction_buffer);
                break;
            case PaddlingState.BackPaddlingRight:
                Debug.Log("Back Paddling right");
                Debug.Log(paddle_direction_buffer);
                break;
            case PaddlingState.BackPaddlingLeft:
                Debug.Log("Back Paddling left");
                Debug.Log(paddle_direction_buffer);
                break;
        }
        //Debug.Log(theta * 180.0 / 3.1416);
    }
}
