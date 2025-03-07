using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MathEquationGenerator : MonoBehaviour
{
    public TMP_Text equationText;
    public TMP_Text answerText;
    public TMP_Text resultText;
    public Button leftButton;
    public Button rightButton;
    public GameObject squarePrefab;

    private int correctAnswer;
    private float checkTimer = 0f;
    private string lastCheckedAnswer = "";
    private float checkDelay = 0.2f;
    private GameObject currentSquare;
    private bool answerMatched = false;

    private Vector3 startPosition = new Vector3(0, 2.5f, 0);
    public float endYPosition = -2.5f;
    private float moveDuration = 10f;

    void Start()
    {
        equationText.text = "";
        answerText.text = "0.00";
        resultText.text = "";

        leftButton.onClick.AddListener(() => OnButtonPressed("left"));
        rightButton.onClick.AddListener(() => OnButtonPressed("right"));

        Invoke("GenerateNewEquation", 0.1f);
    }

    void Update()
    {
        if (answerText.text != lastCheckedAnswer)
        {
            checkTimer = 0f;
            lastCheckedAnswer = answerText.text;
        }
        else
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkDelay)
            {
                CheckAnswer();
                checkTimer = 0f;
            }
        }
    }

    void GenerateNewEquation()
    {
        StartCoroutine(ClearMessageAfterDelay());

        if (currentSquare != null)
        {
            Destroy(currentSquare);
        }

        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        leftButton.interactable = true;
        rightButton.interactable = true;

        int num1 = Random.Range(1, 10);
        int num2 = Random.Range(1, 10);

        if (Random.value > 0.5f)
        {
            correctAnswer = num1 + num2;
            equationText.text ="Package Droping At "+ num1 + " + " + num2 + " = ?";
        }
        else
        {
            correctAnswer = num1 - num2;
            equationText.text ="Package Droping At"+ num1 + " - " + num2 + " = ?";
        }

        float previousAnswerOffset = correctAnswer; // Default offset
        if (float.TryParse(answerText.text, out float previousAnswer)) // Get previous answer before clearing
        {
            previousAnswerOffset = correctAnswer - previousAnswer;
        }

        answerMatched = false;

        GenerateSquare(previousAnswerOffset);
    }

    void CheckAnswer()
    {
        if (!answerMatched && float.TryParse(answerText.text, out float userAnswer))
        {
            if (Mathf.RoundToInt(userAnswer) == correctAnswer)
            {
                answerMatched = true;
                resultText.text = "Package Received";
                Debug.Log("Correct Answer! Generating new equation.");
                StopAllCoroutines();
                GenerateNewEquation();
            }
        }
    }
    void GenerateSquare(float initialXOffset)
    {
        Vector3 spawnPosition = new Vector3(initialXOffset, startPosition.y, startPosition.z);
        currentSquare = Instantiate(squarePrefab, spawnPosition, Quaternion.identity);
        StartCoroutine(MoveSquare(currentSquare));
    }

IEnumerator MoveSquare(GameObject square)
{
    float elapsedTime = 0f;
    Vector3 startPos = new Vector3(correctAnswer, startPosition.y, startPosition.z);
    Vector3 endPos = new Vector3(correctAnswer, endYPosition, startPosition.z);

    while (elapsedTime < moveDuration)
    {
        float userXOffset = correctAnswer;
        if (float.TryParse(answerText.text, out float userAnswer))
        {
            userXOffset = correctAnswer - userAnswer;
        }

        Vector3 currentPos = Vector3.Lerp(startPos, endPos, elapsedTime / moveDuration);
        currentPos.x = userXOffset;
        square.transform.position = currentPos;

        elapsedTime += Time.deltaTime;
        yield return null;
    }

    square.transform.position = endPos;

    if (answerMatched)
    {
        resultText.text = "Package Delivered"; // ✅ New Message for correct answer
    }
    else
    {
        resultText.text = "Package Missed";
    }

    StartCoroutine(ClearMessageAfterDelay());
    GenerateNewEquation();
}

    IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        resultText.text = "";
    }

    void OnButtonPressed(string direction)
    {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
        Debug.Log("Button pressed: " + direction);
    }
}