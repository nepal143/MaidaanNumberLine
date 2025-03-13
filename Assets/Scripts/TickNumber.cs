using UnityEngine;
using TMPro;

public class UpdateTextMesh : MonoBehaviour
{
    public TextMeshProUGUI outputText; // Assign this in the Inspector
    private TextMeshProUGUI centerText; // Reference to CenterText object
    private float lastValue = float.MinValue; // Store last value to detect changes

    void Start()
    {
        // Find the TextMeshPro object with the tag "CenterText"
        GameObject centerTextObj = GameObject.FindGameObjectWithTag("CenterText");

        if (centerTextObj != null)
        {
            centerText = centerTextObj.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("No GameObject found with tag 'CenterText'.");
        }
    }

    void Update()
    {
        if (centerText != null)
        {
            // Try to parse the float value from CenterText
            if (float.TryParse(centerText.text, out float centerValue))
            {
                // Get the X position of this GameObject
                float xPos = transform.position.x;

                // Calculate new value as an integer
                int newValue = Mathf.RoundToInt(centerValue + (2 * xPos));

                // Update only if value changes
                if (Mathf.RoundToInt(centerValue) != Mathf.RoundToInt(lastValue))
                {
                    lastValue = centerValue;
                    if (outputText != null)
                    {
                        outputText.text = newValue.ToString();
                    }
                }
            }
        }
    }
}
