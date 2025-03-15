using UnityEngine;
using TMPro;

public class DifficultyTextUpdater : MonoBehaviour
{
    public TextMeshProUGUI difficultyText; // Assign in Inspector

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
            difficultyText.text = "View the address for the incoming package";
        }
        else
        {
            difficultyText.text = "Solve this to decode the address for the incoming package";
        }
    }
}
