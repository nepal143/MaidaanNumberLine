using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NumberPickerUI : MonoBehaviour
{
    public TMP_Text[] digitTexts = new TMP_Text[5]; // 5 digits (000.00)
    public Button[] upButtons = new Button[5];
    public Button[] downButtons = new Button[5];

    private int[] digitValues = { 0, 0, 0, 0, 0 };

    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            int index = i;
            upButtons[i].onClick.AddListener(() => ChangeValue(index, 1));
            downButtons[i].onClick.AddListener(() => ChangeValue(index, -1));
        }
        UpdateDisplay();
    }

    void ChangeValue(int index, int change)
    {
        digitValues[index] = (digitValues[index] + change + 10) % 10;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        for (int i = 0; i < 5; i++)
        {
            digitTexts[i].text = digitValues[i].ToString();
        }
    }

    public float GetFinalNumber()
    {
        return float.Parse($"{digitValues[0]}{digitValues[1]}{digitValues[2]}.{digitValues[3]}{digitValues[4]}");
    }
}
