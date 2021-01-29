using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

//Author: ZiQi Li
public class PlayerController : MonoBehaviour
{

    [Header("Player motion")]
    [Tooltip("values about the player's speed")]
    //speed of motion
    public float speed = 6f;
    public float viewSpeed = 0.2f; //this speed should be identical with PlayerController's viewSpeed
    //gravity applying to the y-axis
    public float gravity = -9f;
    public float jumpForce = 5f; //up and down
    public CameraController cameraController;  //reference to the CameraController script

    [Header("Ammo")]
    public TextMeshProUGUI ammoCountText; //create a TMP UGUI object to store the count text
    private int ammoCount; // count the current number of projectiles of the player
    private int ammoCollected;  //count the number of projectiles collected by the player in this game

    [Header("Others")]
    public float cameraYoffset; //the offset of camera in Y-axis
    public Rigidbody projectile; //projectile prefab

    //reference to the Character Controller of this object
    private CharacterController playerController;

    //private members to store the player's movements
    private float movementX;
    private float movementY;
    private float movementZ;

    //private members to store the player's view
    private float viewX;
    private float viewY;

    //private members to store the player's condition
    private bool isJumped;
    private bool isGrounded;
    private bool isFired;


    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<CharacterController>();
        isJumped = false;
        isFired = false;
        ammoCollected = 0;
        ammoCount = 0;

    }

    // Update is called once per frame
    void Update()
    {
        //update the rotation of player based on the mouse movement
        updatePlayerRotation(viewX, viewY, viewSpeed);


        //apply a fake gravity all the time to make isGrounded work properly
        //Vector3 fakeGrav = new Vector3(0f, -0.2f, 0f) * Time.deltaTime;
        //this.playerController.Move(fakeGrav);

        isGrounded = playerController.isGrounded;

        //Jumping
        //when the player is grounded, check for Jump input, otherwise apply the gravity
        if (isGrounded)
        {
            if (isJumped)
            {
                //Debug.Log("pressed jump");
                //set the y-movement to 0, and then add the y-movement with the jump movement value 
                movementY = 0f;
                //use "+=", NOT "="
                movementY += Mathf.Sqrt(jumpForce * -3.0f * gravity);
                isJumped = false;

            }
        }
        else  //if not grounded, apply gravity
        {
            //times Time.deltaTime to the gravity to make the falling smooth
            //notice that "+="
            movementY += gravity * Time.deltaTime;
            isJumped = false;
        }

        //update the player motion based on the current view direction using transform.forward and transform.right
        //(note that forward is +z direction, right is +x direction)
        //and also the vertical movement "height"
        Vector3 movement = transform.forward * movementZ * speed;
        movement += transform.right * movementX * speed;
        movement.y = movementY;
        //Do not call Move() more than once in one frame, use a final call on Move()
        this.playerController.Move(movement * Time.deltaTime);


        //handle firing
        fireHandler();
    }

    // Physical updates go here
    // in FixedUpdate(), all motion will * Time.deltaTime automatically
    private void FixedUpdate()
    {

    }

    //create a OnMove function to detect the User keyboard input. (WASD)
    //Note: in Unity, in the "player input" component we sets Actions to InputActions
    //(install InputSystem in Package Manager in "Widow" tab
    //, it's a default Input package that simplify our task)
    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementZ = movementVector.y;

    }

    //create a OnLook function to detect the User mouse movement
    //this function will update the private fields every time the mouse is moving
    void OnLook(InputValue viewMovementValue)
    {
        Vector2 viewVector = viewMovementValue.Get<Vector2>();
        viewX = viewVector.x;
        viewY = viewVector.y;
    }

    void OnJump()
    {
        isJumped = true;
    }

    void OnFire()
    {
        isFired = true;
    }


    //getter method for the ammo remaining for the player
    public int getAmmo()
    {
        return this.ammoCount;
    }

    //getter method for the # of ammo collected in this game
    public int getCollectedAmmo()
    {
        return this.ammoCollected;
    }

    //this function will update the player's view rotation base on the input given in OnLook()
    void updatePlayerRotation(float viewX, float viewY, float viewSpeed)
    {
        //update only the Y-axis rotation, since that's what we need for the moving direction
        this.transform.Rotate(new Vector3(0f, viewX * viewSpeed,0f));
    }


    //this function will generate projectiles when fire
    void fireHandler()
    {
        //only can fire when we have ammo
        if(ammoCount > 0)
        {
            if (isFired)
            {
                Rigidbody projectileInstance;
                //the projectile shooting direction is based on the rotation of camera, not the player itself.
                projectileInstance = Instantiate(projectile, new Vector3(this.transform.position.x, this.transform.position.y + cameraYoffset, this.transform.position.z), cameraController.getCameraRotation()) as Rigidbody;
                projectileInstance.AddForce(cameraController.transform.forward * 1000, ForceMode.Force);
                isFired = false;
                ammoCount--;
                //update the ammo text
                SetAmmoCountText();
            }
        }
        else
        {
            isFired = false;
        }
        
    }

    //function to detect the collision with some specific objects
    private void OnTriggerEnter(Collider other)
    {
        //disable the Ammo object collided, detect using tag system
        //if the collided object is a "Ammo" tag object, make it disappeared
        //remember to set "isTriggered" option for this Collider
        if (other.gameObject.CompareTag("Ammo"))
        {
            other.gameObject.SetActive(false);

            //updated the count text
            ammoCollected++;
            ammoCount++;
            SetAmmoCountText();
        }

    }

    //set the number of ammo text
    void SetAmmoCountText()
    {
        ammoCountText.text = "Ammo: " + ammoCount.ToString();
    }


}
