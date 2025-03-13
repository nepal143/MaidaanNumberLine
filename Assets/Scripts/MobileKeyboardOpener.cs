using UnityEngine;
using TMPro; // Required for TMP_InputField
using UnityEngine.EventSystems;

public class MobileKeyboardOpener : MonoBehaviour, IPointerClickHandler
{
    private TouchScreenKeyboard keyboard;
    public TMP_InputField inputField; // Assign in the Inspector

    void Start()
    {
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>(); // Try to get it dynamically
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inputField == null)
        {
            Debug.LogError("No TMP_InputField assigned or found!");
            return;
        }

        keyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.Default);
    }
}
