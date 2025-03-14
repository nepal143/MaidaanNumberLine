using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameStartManager : MonoBehaviour
{
    public Button startButton;
    public Button startButton1;
    public List<GameObject> objectsToEnable;
    public GameObject gameTitle; // Assign the title GameObject here

    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        if (startButton1 != null)
        {
            startButton1.onClick.AddListener(StartGame);
        }
    }

    public void StartGame()
    {
        // Disable the start button
        startButton.gameObject.SetActive(false);

        // Disable the game title if assigned
        if (gameTitle != null)
        {
            gameTitle.SetActive(false);
        }

        // Enable all objects needed for the game
        foreach (GameObject obj in objectsToEnable)
        {
            obj.SetActive(true);
        }

        Debug.Log("Game Started: Number Line and Elements Activated!");  
    }
}
