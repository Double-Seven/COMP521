using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float viewSpeed = 0.2f;  //this speed should be identical with PlayerController's viewSpeed
    public float y_offset;
    public GameObject player; //ref to the player, to ref it, in Unity drag the Player in Hierarchy to the "Player" in the CameraController script component
    private Vector3 offset;

    //private members to store the player's view
    private float viewX;
    private float viewY;
    private Quaternion camRotation;


    // Start is called before the first frame update
    void Start()
    {
        //the offset is depending on the height we want the player view to be,
        //relative to the player object's position
        offset = new Vector3(0, y_offset, 0);
        camRotation = transform.localRotation;

    }

    // LateUpdate is called once per frame, BUT will be called after
    // all Updates functions have been called
    void LateUpdate()
    {
        //makes the camera match the position of the player
        transform.position = player.transform.position + offset;

        //handle the rotation of camera
        camRotation.x += (-1)* viewY * viewSpeed;
        camRotation.y += viewX * viewSpeed;
        //limit the view vertical angle to a min/max using Mathf.Clamp()
        camRotation.x = Mathf.Clamp(camRotation.x, -70f, 70f);
        transform.localRotation = Quaternion.Euler(camRotation.x, camRotation.y, camRotation.z);
    }

    //this function will update the private fields every time the mouse is moving
    void OnLook(InputValue viewMovementValue)
    {
        Vector2 viewVector = viewMovementValue.Get<Vector2>();
        viewX = viewVector.x;
        viewY = viewVector.y;
    }

    public Quaternion getCameraRotation()
    {
        return this.transform.localRotation;
    }


}
