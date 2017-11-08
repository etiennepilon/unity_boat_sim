using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KanoeController : MonoBehaviour {

    private Rigidbody rb;
    private boatCoordinateManager boat;
    private bool controllingWithOneTouch = true;
    enum PaddlingState {NotPaddling, PaddlingLeft, PaddlingRight, BackPaddlingLeft, BackPaddlingRight};
    // --
    // These values are to determine if the person is trying to circle
    private double incrementalTurn = 0;
    private bool goingLeft = false, goingRight = false;
    // --
    // All variables related to holding a Paddle
    private bool holdingPaddle = true;
    private double paddleAngleThreshold = 15.0 * 3.1416 / 180.0; // This is 10 degrees
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
    }
    void FixedUpdate()
    {
        PaddlingState state = getPaddlingState();
        double paddleDistance = 0.9;
        if (state == PaddlingState.PaddlingLeft) boat.paddleLeftWithCorrectionFactor(paddleDistance);
        else if (state == PaddlingState.PaddlingRight) boat.paddleRightWithCorrectionFactor(paddleDistance);
        else if (state == PaddlingState.BackPaddlingLeft) boat.backPaddleLeftWithPaddleDistance(paddleDistance);
        else if (state == PaddlingState.BackPaddlingRight) boat.backPaddleRightWithPaddleDistance(paddleDistance);
        else boat.notPaddlingWithCorrection();

        rb.MovePosition(new Vector3(boat.p_y, 0, boat.p_x));
        Quaternion rotation = Quaternion.Euler(new Vector3(0, (float)(180.0 / 3.1416 * boat.heading), 0));
        rb.MoveRotation(rotation);
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
        if (controllingWithOneTouch && holdingPaddle)
        {
            // -- TODO
            state = getOneTouchPaddlingState();
        }
        else if (controllingWithOneTouch && !holdingPaddle)
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
        //updateTurningIncrement(state);
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
        //Debug.Log(theta * 180.0/3.1416);
        if (paddleIsInTheWater(theta) && paddleIsRightSide(theta))
        {
           if (paddleIsPushingWaterBack(previousHandCoords.zright - currentHandCoords.zright))
           {
                state = PaddlingState.PaddlingRight;
           }
            else if (paddleIsPushingWaterForward(previousHandCoords.zleft - currentHandCoords.zleft))
            {
                state = PaddlingState.BackPaddlingRight;
            }
        }
        else if (paddleIsInTheWater(theta) && paddleIsLeftSide(theta))
        {
           if (paddleIsPushingWaterBack(previousHandCoords.zleft - currentHandCoords.zleft))
           {
                state = PaddlingState.PaddlingLeft;
            }
           else if (paddleIsPushingWaterForward(previousHandCoords.zleft - currentHandCoords.zleft))
            {
                state = PaddlingState.BackPaddlingLeft;
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
}
