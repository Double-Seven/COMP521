using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: ZiQi Li
public class Projectile : MonoBehaviour
{
    private GameFlowManager gameFlowManager;

    // Start is called before the first frame update
    void Start()
    {
        //create a reference to the GameManager object, we cannot reference it in a public field,
        //since this script is attached to a prefab object.
        GameObject gm = GameObject.Find("GameManager");
        gameFlowManager = (GameFlowManager) gm.GetComponent("GameFlowManager");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //function to detect the collision with some specific objects (depending on the tags)
    private void OnTriggerEnter(Collider other)
    {
        //disable the projectile object collided, detect using tag system
        //if the collided object is a "Platform" tag object, make it disappeared
        //remember to set "isTriggered" option for this Collider
        if (other.gameObject.CompareTag("Platform"))
        {
            //due to I set the single platform object as the parent object of walls, we can't
            //directly set the platform to invisible, since it will set the walls to invisible too
            //so we have to change the MeshRenderer of the single platform
            ((MeshRenderer)other.gameObject.GetComponent("MeshRenderer")).enabled = false;

            //Since for platform we have one triggered BoxCollider for the collision between projectile and the platform
            // and one non-triggered BoxCollider for the physical interaction between player and platform.
            //We have to disable both of them
            BoxCollider[] bcs = (other.gameObject.GetComponents<BoxCollider>());
            foreach (BoxCollider bc in bcs)
            {
                bc.enabled = false;
            }

            //destroy the projectile after collision with platform
            Destroy(this.gameObject);

            //call the OnDestroying(GameObject destroyedPlatform) function in the gameFlowManager to pass the destroyed platform to the gameFlowManager
            gameFlowManager.OnDestroying(other.gameObject);

        }
    }

    //function to detect the collision with collider or rigidbody
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Player"))
        {
            //do nothing, since the projectile spawn inside the player body
        }
        else
        {
            //if the projectile collides with other objects, simply destroy the projectile
            Destroy(this.gameObject);
        }
    }

}
