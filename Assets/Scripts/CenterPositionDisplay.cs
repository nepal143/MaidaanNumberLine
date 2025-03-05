using UnityEngine;
using TMPro;
using System.Collections;

public class CenterPositionDisplay : MonoBehaviour
{
    public TextMeshProUGUI positionText; // Assign the UI Text
    public TMP_InputField inputField; // Reference to the input field
    public float moveDuration = 1f; // Always update in 1 second
    public float animationSpeed = 2f; // Speed of scale animation

    private float targetPosition = 0f; // The calculated position
    private bool isAnimating = false;

    void Start()
    {
        UpdateText(targetPosition);
    }

    public void OnMoveTriggered()
    {
        if (float.TryParse(inputField.text, out float moveValue))
        {
            float newPosition = targetPosition + moveValue; // Calculate based on input
            StartCoroutine(SmoothUpdateText(targetPosition, newPosition));
            targetPosition = newPosition;
        }
        else
        {
            Debug.LogWarning("Invalid input. Enter a number.");
        }
    }

    IEnumerator SmoothUpdateText(float startValue, float endValue)
    {
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / moveDuration);
            UpdateText(currentValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        UpdateText(endValue); // Ensure the final value is accurate
        StartCoroutine(AnimateText());
    }

    void UpdateText(float value)
    {
        positionText.text = $"{value:F2}";
    }

    IEnumerator AnimateText()
    {
        if (isAnimating) yield break;
        isAnimating = true;

        Vector3 originalScale = positionText.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            positionText.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime * animationSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            positionText.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime * animationSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        positionText.transform.localScale = originalScale;
        isAnimating = false;
    }
}
