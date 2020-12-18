using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Author: ZiQi Li
public class CrateDestroyer : MonoBehaviour
{
    private GameFlowManager gameFlowManager;

    public bool isDestroyed { get; set; } = false;  // Note: GameObjects get destroyed at the end of the current frame by default, so we need set a bool variable manually

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
        
        // delete this crate from the game scene after being thrown by monster and colliding with ground
        if (this.GetComponent<Rigidbody>().velocity.magnitude > 0.1f && other.gameObject.tag != "Monster" && other.gameObject.tag != "Rock" && other.gameObject.tag != "Crate")
        {
            gameFlowManager.obstacles_to_avoid.Remove(this.gameObject);
            Destroy(this.gameObject);
            //this.isDestroyed = true;

        }


    }
}
