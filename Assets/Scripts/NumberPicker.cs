using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NumberPickerUI : MonoBehaviour
{
    public TMP_Text displayText; // Shows the full number
    public Button[] numberButtons; // 0-9 buttons
    public Button decimalButton; // Decimal point button
    public Button backspaceButton; // Backspace button
    public Button clearButton; // Clear button
    public Button clearButton1; 

    private string currentNumber = "Steps To Move"; // Holds the entered number

    void Start()
    { 
        // Assign number buttons (0-9)
        for (int i = 0; i < numberButtons.Length; i++)
        {
            int digit = i;
            numberButtons[i].onClick.AddListener(() => AddDigit(digit.ToString()));
        }

        // Assign decimal button
        decimalButton.onClick.AddListener(() => AddDigit("."));

        // Assign backspace button with delay
        backspaceButton.onClick.AddListener(() => StartCoroutine(DelayedRemoveLastDigit()));

        // Assign clear buttons with delay
        clearButton.onClick.AddListener(() => StartCoroutine(DelayedClearInput()));
        clearButton1.onClick.AddListener(() => StartCoroutine(DelayedClearInput()));

        UpdateDisplay();
    }

    void AddDigit(string digit)
    {
        // Remove "Steps To Move" when first typing
        if (currentNumber == "Steps To Move")
            currentNumber = "";

        // Prevent multiple decimals
        if (digit == "." && currentNumber.Contains(".")) return;

        // Prevent leading zeros like "02"
        if (currentNumber == "0" && digit != ".")
            currentNumber = digit;
        else
            currentNumber += digit;

        UpdateDisplay();
    }

    IEnumerator DelayedRemoveLastDigit()
    {
        yield return new WaitForSeconds(0.2f);
        RemoveLastDigit();
    }

    void RemoveLastDigit()
    {
        if (currentNumber.Length > 1)
            currentNumber = currentNumber.Substring(0, currentNumber.Length - 1);
        else
            currentNumber = "Steps To Move"; // Reset to default if empty

        UpdateDisplay();
    }

    IEnumerator DelayedClearInput()
    {
        yield return new WaitForSeconds(0.2f);
        ClearInput();
    }

    void ClearInput()
    {
        currentNumber = "Steps To Move";
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        displayText.text = currentNumber;
    }

    public float GetFinalNumber()
    {
        return float.TryParse(currentNumber, out float result) ? result : 0f;
    }
}
