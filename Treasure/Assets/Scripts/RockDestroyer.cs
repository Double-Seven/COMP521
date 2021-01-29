using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Author: ZiQi Li
public class RockDestroyer : MonoBehaviour
{
    private GameFlowManager gameFlowManager;

    // Start is called before the first frame update
    void Start()
    {
        gameFlowManager = GameObject.FindWithTag("GameController").GetComponent<GameFlowManager>(); // get the instance of the game flow manager script
    }

    // Update is called once per frame
    void Update()
    {

    }


    /// <summary>
    /// Detection of the collision
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        // delete this rock from the game scene after being thrown by monster and colliding with player
        // Note: this rock will have a velocity when being thrown 
        if (this.GetComponent<Rigidbody>().velocity.magnitude >= 0.05f && other.tag == "Player")
        {
            gameFlowManager.obstacles_to_avoid.Remove(this.gameObject);
            Destroy(this.gameObject);
        }


    }
}
