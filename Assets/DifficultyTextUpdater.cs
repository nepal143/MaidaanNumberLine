using UnityEngine;
using TMPro;

public class DifficultyTextUpdater : MonoBehaviour
{
    public TextMeshProUGUI difficultyText; // Assign in Inspector
    public string Level4 ; 
    public string above4 ; 

    void Start()
    {
        if (WebGLBridge.Instance == null)
        {
            Debug.LogError("WebGLBridge instance is not found!");
            return;
        }

        UpdateDifficultyText();
    }

    void UpdateDifficultyText()
    {
        if (WebGLBridge.Instance.baseDifficulty == 4)
        {
            difficultyText.text = Level4;
        }
        else
        {
            difficultyText.text =  above4;
        }
    }
}
