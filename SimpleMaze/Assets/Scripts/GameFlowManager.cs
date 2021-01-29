using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Author: ZiQi Li
public class GameFlowManager : MonoBehaviour
{
    [Header("Text")]
    public TextMeshProUGUI winText;
    public TextMeshProUGUI loseText;
    public Button playAgainButton;

    [Header("Player")]
    public GameObject player;
    public PlayerController playerController;

    [Header("Terrain values")]
    public float canyonBottom;  //the y-axis value at the bottom of canyon
    public float mazeExit;      //the z-axis value at the exit of maze

    [Header("Maze")]
    public InitializationManager initManager;


    private GameObject destroyedPlatform = null;  //store the platform newly destroyed by projectiles
    private bool isSolutionDestroyed = false;

    // Start is called before the first frame update
    void Start()
    {
        //make the cursor invisible and lock the cursor in the window
        //Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        

        //set win text and lose text and button to invisible at the beginning
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        playAgainButton.gameObject.SetActive(false);

        //setup the button and bind with playAgainButtonOnClick() function
        Button paButton = playAgainButton.GetComponent<Button>();
        paButton.onClick.AddListener(playAgainButtonOnClick);
    }


    // Update is called once per frame
    void Update()
    {
        //check whether a solution platform of the maze has been destroyed by comparing the newly destroyed platform with the solution array
        isSolutionDestroyed = initManager.getMazeSolution().Contains(this.destroyedPlatform);

        //check winning condition first
        //if the player has crossed the maze (canyon) and eliminate at least one platform belongs to the solution of maze, prompt win text
        if (player.transform.position.z >= mazeExit && isSolutionDestroyed)
        {
            winPrompt();
            //no further action can be perform after winning
            player.SetActive(false);
        }
        else
        {
            //losing condition check
            //if the player falls in the canyon
            if (player.transform.position.y <= canyonBottom)
            {
                losePrompt();
                //no further action can be perform after losing
                player.SetActive(false);
            }
            //if the player used all ammo in the map and not eliminated a solution platform
            else if (playerController.getAmmo() == 0 && playerController.getCollectedAmmo() == initManager.getTotalAmmo() && isSolutionDestroyed == false)
            {
                losePrompt();
                //no further action can be perform after losing
                player.SetActive(false);
            }
            //if the player eliminate a solution platform //(won't be able to get to the other side of canyon)
            else if(isSolutionDestroyed && player.transform.position.z < mazeExit)
            {
                losePrompt();
                //no further action can be perform after losing
                player.SetActive(false);
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
        //show the winning text and playAgain button
        winText.gameObject.SetActive(true);
        playAgainButton.gameObject.SetActive(true);
        setCursorUnlocked();
    }

    //when the losing condition is triggered
    void losePrompt()
    {
        //show the losing text and playAgain button
        loseText.gameObject.SetActive(true);
        playAgainButton.gameObject.SetActive(true);
        setCursorUnlocked();
    }

    //calling from Projectile script, setting the destroyed platform field to the newly destroyed platform
    public void OnDestroying(GameObject destroyedPlatform)
    {
        this.destroyedPlatform = destroyedPlatform;
    }




}
