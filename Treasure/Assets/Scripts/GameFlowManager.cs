using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


// Author: ZiQi Li
public class GameFlowManager : MonoBehaviour
{
    // Public variables
    public Camera gameOverCamera;

    [Header("UI elements")]
    public TMP_Text defValue_text;
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text treasureText;
    public Button playAgainButton;

    // Script instances
    [Header("Script instances")]
    public PlayerContoller playerController;
    public MonsterController monsterController;
    public GameInitializer gameInit;


    // Map objects (used to determine Winning state of the game)
    [Header("Map objects")]
    public GameObject treasureGameObject;
    public GameObject entranceGameObject;
    public GameObject[] cavePoints;
    public GameObject walls;

    // Steering behavior of slimes
    [Header("Steering Force of slimes")]
    public float fleeWeight = 0;  // how large the flee force will have impact on the motion of slimes
    public float wanderWeight = 0;  // how large the wander force will have impact on the motion of slimes

    // ------------------------------





    // Private variables
    // Player game state
    private bool hasTreasure = false;  // bool stores whether the player got the treasure

    public ArrayList agents_to_avoid { get; set; } = new ArrayList();  // list of moving agent to avoid by the slimes 
    public ArrayList obstacles_to_avoid { get; set; }
    public ArrayList spots_to_avoid { get; set; } = new ArrayList();  // entrance and cave to avoid
    public ArrayList slime_to_avoid { get; set; }
    public ArrayList walls_to_avoid { get; set; } = new ArrayList();  // walls to avoid

    // ------------------------------







    // Start is called before the first frame update
    void Start()
    {
        // Initializations:

        //-------For UI----------
        // disable gameOver camera
        gameOverCamera.enabled = false;

        //set win text and lose text and button to invisible at the beginning
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        playAgainButton.gameObject.SetActive(false);

        //setup the button and bind with playAgainButtonOnClick() function
        Button paButton = playAgainButton.GetComponent<Button>();
        paButton.onClick.AddListener(playAgainButtonOnClick);
        //-------For UI----------


        //-------For steering force----------
        // add objects to avoid for slimes
        agents_to_avoid.Add(this.playerController.gameObject);
        agents_to_avoid.Add(this.monsterController.gameObject);

        obstacles_to_avoid = gameInit.getObstacleList();

        // prevent slimes entering cave
        foreach(GameObject cavePoints in cavePoints)
        {
            spots_to_avoid.Add(cavePoints);
        }
        spots_to_avoid.Add(entranceGameObject);
        spots_to_avoid.Add(treasureGameObject);

        slime_to_avoid = gameInit.getSlimeList();

        // add all walls
        foreach(Transform child in this.walls.transform)
        {
            walls_to_avoid.Add(child.gameObject);
        }
        //-------For steering force----------
    }


    // Non physical update
    private void Update()
    {
        // update the defense value of player on UI 
        updateDefValue(this.playerController);

        // check player game state (winning / losing)
        checkGameState();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        updateSlimesMotion(gameInit.getSlimeList(), Time.deltaTime);
    }



    /// <summary>
    /// Function to update the slimes' motion using Steering Forces
    /// </summary>
    /// <param name="slimeList"></param>
    void updateSlimesMotion(ArrayList slimeList, float deltaTime)
    {
        foreach(GameObject slime in slimeList)
        {
            Rigidbody slimeRB = slime.GetComponent<Rigidbody>();
            SlimeController slimeController = slime.GetComponent<SlimeController>();

            slimeRB.AddForce(slimeController.flee() * fleeWeight);  // apply flee force 

            slimeRB.AddForce(slimeController.wander(Time.deltaTime) * wanderWeight);  // apply wander force

            //Debug.Log(slimeRB.velocity.y);

            //Debug.Log(slimeRB.velocity);
            // make slime facing its moving direction when its moving
            if (slimeRB.velocity.magnitude > 0.01f)
            {
                Vector3 facingDirection = slimeRB.velocity;
                facingDirection.y = slimeRB.transform.position.y;

                //Quaternion desiredRotation = Quaternion.LookRotation(facingDirection);
                //slimeRB.transform.rotation = Quaternion.Slerp(slimeRB.transform.rotation, desiredRotation, deltaTime);
                //slimeRB.transform.rotation = Quaternion.LookRotation(facingDirection, Vector3.up);
                //slimeRB.MoveRotation(Quaternion.LookRotation(slimeRB.velocity));
                //slimeRB.transform.LookAt(slimeRB.transform.position + facingDirection);
            }
            
        }
    }




    /// <summary>
    /// Function to update the defense value of player on UI
    /// </summary>
    /// <param name="pc"></param>
    void updateDefValue(PlayerContoller pc)
    {
        int defValue = pc.getDefenseValue();
        if(defValue >= 0)
        {
            defValue_text.SetText("Defense Value: " + defValue);
        }
    }


    /// <summary>
    /// Check the game state of the player (Winning/Losing)
    /// And end the game accordingly
    /// </summary>
    void checkGameState()
    {
        // if player hitten twice by projectiles, losing the game
        if(playerController.isPlayerLost)
        {
            losePrompt();
        }
        // check is the player is close to the treasure
        else if(!hasTreasure)
        {
            // if close enough to the treasure, we consider that the player gets the treasure
            if(Vector3.Distance(this.playerController.gameObject.transform.position, this.treasureGameObject.transform.position) <= 2)
            {
                this.hasTreasure = true;
                // set UI text
                treasureText.SetText("Treasure: 1/1");
            }
        }
        else  // when player got the treasure, check if the player is back to entrance
        {
            // if close enough to the entrance, we consider that the player wins this game
            if (Vector3.Distance(this.playerController.gameObject.transform.position, this.entranceGameObject.transform.position) <= 2)
            {
                this.hasTreasure = true;
                winPrompt();
                
            }
        }
        
    }


    //button action listenner function for PlayAgainButton
    void playAgainButtonOnClick()
    {
        //reload the whole scene when pressing the playAgainButton
        SceneManager.LoadScene("MainGame");
    }

    //set the cursor to be unlocked and visible
    void setCursorUnlocked()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    //when the winning condition is triggered
    void winPrompt()
    {
        // deactivate the player moving sript
        this.playerController.gameObject.GetComponent<FirstPersonAIO>().enabled = false;
        this.playerController.gameObject.SetActive(false);
        this.gameOverCamera.enabled = true;

        //show the winning text and playAgain button
        winText.gameObject.SetActive(true);
        playAgainButton.gameObject.SetActive(true);
        setCursorUnlocked();
        Cursor.visible = true;
    }

    //when the losing condition is triggered
    void losePrompt()
    {
        // deactivate the player moving sript
        this.playerController.gameObject.GetComponent<FirstPersonAIO>().enabled = false;

        //show the losing text and playAgain button
        loseText.gameObject.SetActive(true);
        playAgainButton.gameObject.SetActive(true);
        setCursorUnlocked();
        Cursor.visible = true;
    }



}
