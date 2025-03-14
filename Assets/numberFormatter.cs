using UnityEngine;
using TMPro;

public class NumberFormatter : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;

    public void UpdateText(float number)
    {
        string formattedNumber = number.ToString("0.00");

        // Remove ".00" if not exactly "0.00"
        if (formattedNumber != "0.00" && formattedNumber.EndsWith(".00"))
        {
            formattedNumber = formattedNumber.Replace(".00", "");
        }

        textMeshPro.text = formattedNumber;
    }
}
