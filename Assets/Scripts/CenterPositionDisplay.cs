using UnityEngine;
using TMPro;
using System.Collections;

public class CenterPositionDisplay : MonoBehaviour
{
    public TextMeshProUGUI positionText;
    public NumberPickerUI numberPicker;
    public float moveDuration = 1f;
    public float animationSpeed = 2f;

    private float targetPosition = 0f;
    private int initialPosition = 0;
    private bool isAnimating = false;

    void Start()
    {
        UpdateText(initialPosition);
    }

    public void MoveRight()
    {
        float moveValue = numberPicker.GetFinalNumber();
        float newPosition = targetPosition + moveValue;
        StartCoroutine(SmoothUpdateText(targetPosition, newPosition));
        targetPosition = newPosition;
    }

    public void MoveLeft()
    {
        float moveValue = numberPicker.GetFinalNumber();
        float newPosition = targetPosition - moveValue;
        StartCoroutine(SmoothUpdateText(targetPosition, newPosition));
        targetPosition = newPosition;
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

        UpdateText(endValue);
        StartCoroutine(AnimateText());
    }

    void UpdateText(float value)
    {
        if (Mathf.Approximately(value % 1, 0)) // Check if it's a whole number
            positionText.text = $"{(int)value}";
        else
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
