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
    public GameObject blastPrefab; // Explosion Effect
    public AudioClip correctSound;
    public AudioClip wrongSound;
    private AudioSource audioSource;

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
        answerText.text = "0.00"; // Ensure the user input is always a float
        resultText.text = "";
        audioSource = GetComponent<AudioSource>();

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
            equationText.text = num1 + " + " + num2 + " = ?";
        }
        else
        {
            correctAnswer = num1 - num2;
            equationText.text = num1 + " - " + num2 + " = ?";
        }

        float previousAnswerOffset = correctAnswer;
        if (float.TryParse(answerText.text, out float previousAnswer))
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
            // Compare the user input as float (not int)
            if (Mathf.Approximately(userAnswer, correctAnswer)) // Handles small floating point errors
            {
                answerMatched = true;
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
                userXOffset = correctAnswer - userAnswer; // Allow decimal values
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
            resultText.text = "Package Received";
            ColorUtility.TryParseHtmlString("#CCF900", out Color correctColor);
            resultText.color = correctColor; // Set color to #CCF900 (yellowish)
            audioSource.PlayOneShot(correctSound);
        }
        else
        {
            resultText.text = "Package Missed";
            resultText.color = Color.red; // Change text color to red when package is missed
            SpawnExplosion(square.transform.position);
            audioSource.PlayOneShot(wrongSound);

            yield return new WaitForSeconds(0.5f);
        }

        GenerateNewEquation();
    }

    void SpawnExplosion(Vector3 position)
    {
        GameObject blast = Instantiate(blastPrefab, position, Quaternion.identity);
        Destroy(blast, 1.5f); // Ensure blast effect lasts 1.5 seconds
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
