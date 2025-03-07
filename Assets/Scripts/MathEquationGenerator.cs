using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MathEquationGenerator : MonoBehaviour
{
    public TMP_Text equationText; // Reference to UI Text where the equation is displayed
    public TMP_Text answerText;   // Reference to the final answer field below
    public TMP_Text resultText;   // Reference to the result message text field
    public Button leftButton; // Reference to the left button
    public Button rightButton; // Reference to the right button
    public GameObject squarePrefab; // Reference to the square prefab

    private int correctAnswer;
    private float checkTimer = 0f;
    private string lastCheckedAnswer = "";
    private float checkDelay = 0.2f; // Time in seconds before validating the answer
    private GameObject currentSquare;
    private bool answerMatched = false;

    void Start()
    {
        GenerateNewEquation();
        leftButton.onClick.AddListener(() => OnButtonPressed("left"));
        rightButton.onClick.AddListener(() => OnButtonPressed("right"));
    }

    void Update()
    {
        if (answerText.text != lastCheckedAnswer)
        {
            checkTimer = 0f; // Reset the timer when the answer changes
            lastCheckedAnswer = answerText.text;
        }
        else
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkDelay)
            {
                CheckAnswer();
                checkTimer = 0f; // Reset after checking
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

        int num1 = Random.Range(1, 10); // Generate random numbers between 1 and 9
        int num2 = Random.Range(1, 10);
        
        if (Random.value > 0.5f)
        {
            correctAnswer = num1 + num2;
            equationText.text = num1 + " + " + num2 + " = ?";
        }
        else
        {
            correctAnswer = num1 - num2;
            equationText.text = num1 + " - " + num2 + " = ?";
        }
        
        answerMatched = false;
        GenerateSquare();
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
                StopAllCoroutines(); // Stop the square from moving if answered correctly
                GenerateNewEquation();
            }
        }
    }

    void GenerateSquare()
    {
        currentSquare = Instantiate(squarePrefab, new Vector3(0, 2.5f, 0), Quaternion.identity);
        StartCoroutine(MoveSquareDown(currentSquare));
    }

    IEnumerator MoveSquareDown(GameObject square)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = square.transform.position;
        Vector3 endPosition = new Vector3(0, 0, 0);
        
        while (elapsedTime < 10f)
        {
            if (answerMatched)
                yield break; // Stop moving if the answer was correct

            square.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / 10f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        square.transform.position = endPosition;

        if (!answerMatched)
        {
            resultText.text = "Package Missed";
            StartCoroutine(ClearMessageAfterDelay());
            GenerateNewEquation();
        }
    }

    IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        resultText.text = "";
    }

    void OnButtonPressed(string direction)
    {
        Debug.Log("Button pressed: " + direction);
        // Implement movement logic if needed
    }
}
