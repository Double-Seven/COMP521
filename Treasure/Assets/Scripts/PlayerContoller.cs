using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Author: ZiQi Li
public class PlayerContoller : MonoBehaviour
{

    // Public variable
    public GameObject shieldGameObject;
    public int defenseValue = 0;
    public GameFlowManager gameFlowManager;
    // -------------------------------

    // Private variable
    private float def_TimeCounter = 0;  // time counter for defense state
    private bool canPressDef = true;  // keep track the pressing status of Defense key

    // Player game state
    private int num_hit = 0;  // number of times that player is hitten by monster (without defense)
    private float collisionColddown = 1.5f;  // set a 1.5seconds colddown for the collision damage taken by player (so the projectiles won't hit player twice in 2 frames)
    public bool isPlayerLost { set; get; } = false;  // bool stores whether the player lost the game (got hit twice by monster (without defense))


    // -------------------------------






    // Start is called before the first frame update
    void Start()
    {
        // initialize the shield to invisible
        this.shieldGameObject.SetActive(false);
    }

    // non physics update
    private void Update()
    {
        updateDefense(Time.deltaTime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        collisionColddown += Time.deltaTime;
    }


    /// <summary>
    /// Function return the current defense value of player
    /// </summary>
    /// <returns>int</returns>
    public int getDefenseValue()
    {
        return this.defenseValue;
    }

    /// <summary>
    /// Function to check whether the player toggle "Defense" mode using defense key
    /// </summary>
    void updateDefense(float deltaTime)
    {
        // if the shield is activated, reduce the defValue by 1 each second
        if (this.shieldGameObject.activeInHierarchy)
        {
            this.def_TimeCounter += deltaTime;

            // reduce the defValue by 1 each second
            if (this.def_TimeCounter >= 1)
            {
                this.def_TimeCounter = 0;  // reset the time counter
                this.defenseValue--;

                // deactivate shield if no defense value left
                if(defenseValue < 1)
                {
                    this.shieldGameObject.SetActive(false);
                }
            }
            
        }

        // set canPressDef to true when the Defense key is released after being pressed 
        if (Input.GetButtonUp("Defense"))
        {
            canPressDef = true;
        }

        // If canPressDef is true (means the key has been released):
        // active shield if shield is not yet activated, defValue > 0 and Defense key is pressed ( can be set in ProjectSetting-InputSystem-Defense, I set it to spacebar)
        if (canPressDef && !this.shieldGameObject.activeInHierarchy && this.defenseValue > 0 && Input.GetButton("Defense"))
        {
            this.shieldGameObject.SetActive(true);
            canPressDef = false;
        }
        // deactivate shield if shield is activated and Defense key is pressed
        else if (canPressDef && this.shieldGameObject.activeInHierarchy && Input.GetButton("Defense"))
        {
            this.shieldGameObject.SetActive(false);
            canPressDef = false;
        }
    }



    /// <summary>
    /// Detection of the collision with slime
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        // if hitten by projectiles without defense, increment num_hit
        if (other.gameObject.tag == "Rock" || other.gameObject.tag == "Crate")
        {
            // if the collider is moving (projectile)
            if (other.gameObject.GetComponent<Rigidbody>().velocity.magnitude > 0.1f)
            {
                if (this.shieldGameObject.activeSelf == false && collisionColddown >=1.5f)
                {
                    num_hit++;
                    collisionColddown = 0;  // reset the collision colddown
                    if (num_hit >= 2)
                        isPlayerLost = true;  // player loses if being hitten twice without shield
                }
            }
        }
        

    }
}
