using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SomeEnum;

//Author: ZiQi Li, for Comp521 A2, McGill University
public class PlayerController : MonoBehaviour
{
    [Header("Player Cannons")]
    public GameObject cannonBallPrefab;
    public GameObject leftCannon;
    public GameObject rightCannon;
    public Text muzzleVelocityText; //create a TMP UGUI object to store the muzzle velocity
    public InputActionAsset asset;  //the input asset, using to get input actions

    //private variables for cannons states
    private bool isLeftCannon = true;  //when true, select the left cannon, when false select the right cannon 
    private bool isFired = false;
    private float cannonAngle;  //angle of the cannon relative to the ground
    private float muzzleVelocity = 1f; //initial velocity of the cannon ball
    private float maxMuzzleVelocity = 20f;

    private InputAction cannonAngleAction;

    private ArrayList cannonBallsList;  //array list to store the shoot cannon balls, using for updating their position, velocity and collision


    // Start is called before the first frame update
    void Start()
    {
        SetMuzzleVelocityText();
        //get the "inputAction" of CannonElevation for checking pressed state of up and down key
        cannonAngleAction = asset.FindAction("CannonElevation");
        cannonBallsList = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        //update the muzzle velocity text
        SetMuzzleVelocityText();
        //update the cannon elevation
        updateCannonElevation();

        //handle the fire action
        fireHandler();


    }






    //create a OnFire function to detect the User keyboard input. (space)
    //Note: in Unity, in the "player input" component we sets Actions to InputActions
    //(install InputSystem in Package Manager in "Widow" tab
    //, it's a default Input package that simplify our task)
    void OnFire()
    {
        isFired = true;
    }

    //this function will generate cannon ball when fire key is pressed
    void fireHandler()
    {

        if (isFired)
        {
            if (isLeftCannon)
            {
                GameObject cannonBall = Instantiate(cannonBallPrefab, leftCannon.transform.GetChild(0).position, leftCannon.transform.GetChild(0).rotation);
                CannonBall cb = new CannonBall(cannonBall, leftCannon.transform.GetChild(0).position, muzzleVelocity, MuzzleDirection.RIGHT, leftCannon.transform.rotation.eulerAngles.z);
                cannonBallsList.Add(cb);
            }
            else
            {
                GameObject cannonBall = Instantiate(cannonBallPrefab, rightCannon.transform.GetChild(0).position, rightCannon.transform.GetChild(0).rotation);
                CannonBall cb = new CannonBall(cannonBall, rightCannon.transform.GetChild(0).position, muzzleVelocity, MuzzleDirection.LEFT, rightCannon.transform.rotation.eulerAngles.z);
                cannonBallsList.Add(cb);
            }
            isFired = false;
        }
        
    }

    //detect the keyboard input for switching cannon
    void OnSwitchCannon()
    {
        isLeftCannon = !isLeftCannon;
    }

    //detect the keyboard input for cannon elevation
    void OnCannonElevation(InputValue elevation)
    {
        float angle = elevation.Get<float>();
        //variable = (condition) ? expressionTrue : expressionFalse;
        //GameObject currentCannon = isLeftCannon ? leftCannon : rightCannon;

        cannonAngle += angle;

        //Debug.Log(cannonAngle);
    }

    //this function will change the cannons elevation
    void updateCannonElevation()
    {
        //change the state for cannon (left or right cannon)
        if (isLeftCannon)
        {
            
            //cannonAngle = Mathf.Clamp(cannonAngle, 0, 90);
            
            //change cannon elevation angle in range (0, 90), note that we have to convert degree to Quaternion to do comparison
            //and only rotate when the key is pressed
            if (cannonAngleAction.phase == InputActionPhase.Started && (leftCannon.transform.rotation.z + Quaternion.Euler(0,0,cannonAngle).z) < Quaternion.Euler(0, 0, 90f).z && (leftCannon.transform.rotation.z + Quaternion.Euler(0, 0, cannonAngle).z) >= Quaternion.Euler(0, 0, 0f).z)
            {
                
                leftCannon.transform.Rotate(0, 0, cannonAngle);
            }
            else
            {
                cannonAngle = 0f;
            }
        }
        else
        {
            //change cannon elevation angle in range (-90, 0) since it's the right cannon
            //and only rotate when the key is pressed
            if (cannonAngleAction.phase == InputActionPhase.Started && (rightCannon.transform.rotation.z + Quaternion.Euler(0, 0, -cannonAngle).z) <= Quaternion.Euler(0, 0, 0f).z && (rightCannon.transform.rotation.z + Quaternion.Euler(0, 0, -cannonAngle).z) > Quaternion.Euler(0, 0, -90f).z)
            {
                //here we rotate by -cannonAngle since the right cannon's angle is opposite to the left cannon
                rightCannon.transform.Rotate(0, 0, -cannonAngle);
            }
            else
            {
                cannonAngle = 0f;
            }
        }
    }

    //detect the keyboard input for muzzle velocity
    void OnMuzzleVelocity(InputValue muzzleV)
    {
        float velocity = muzzleV.Get<float>();

        if (muzzleVelocity + velocity >= maxMuzzleVelocity)
        {
            muzzleVelocity = maxMuzzleVelocity;
        }
        else if (muzzleVelocity + velocity > 0)
        {
           muzzleVelocity += velocity;
        }
        else
        {
            return;
        }
    }

    //set the muzzle velocity text
    void SetMuzzleVelocityText()
    {
        if(muzzleVelocity == maxMuzzleVelocity)
        {
            muzzleVelocityText.text = "Current muzzle velocity: " + muzzleVelocity.ToString() + " m/s (MAX)";
        }
        else
        {
            muzzleVelocityText.text = "Current muzzle velocity: " + muzzleVelocity.ToString() + " m/s";
        }
        
    }

    //getter function for the list that stores cannon balls
    public ArrayList getCannonBallsList()
    {
        return this.cannonBallsList;
    }


}
