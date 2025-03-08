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
    public GameObject blastPrefab;
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

    private float moveSpeed = 0.5f;
    public float speedMultiplier = 1f;  // Speed multiplier, can be adjusted anytime

    void Start()
    {
        equationText.text = "";
        answerText.text = "0.00";
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
        ResetSpeedMultiplier();  // Reset speed before generating a new equation
        StartCoroutine(ClearMessageAfterDelay());

        if (currentSquare != null)
        {
            Destroy(currentSquare);
        }

        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        leftButton.interactable = true;
        rightButton.interactable = true;

        float previousAnswer = 0f;
        if (float.TryParse(answerText.text, out float userAnswer))
        {
            previousAnswer = userAnswer;
        }

        do
        {
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

        } while (Mathf.Abs(correctAnswer - previousAnswer) <= 3);

        answerMatched = false;
        GenerateSquare(correctAnswer - previousAnswer);
    }

    void ResetSpeedMultiplier()
    {
        speedMultiplier = 1f;  // Reset speed back to normal before generating a new equation
    }


    void CheckAnswer()
    {
        if (!answerMatched && float.TryParse(answerText.text, out float userAnswer))
        {
            if (Mathf.Approximately(userAnswer, correctAnswer))
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
        while (square.transform.position.y > endYPosition)
        {
            if (square == null) yield break;

            float userXOffset = correctAnswer;
            if (float.TryParse(answerText.text, out float userAnswer))
            {
                userXOffset = correctAnswer - userAnswer;
            }

            Vector3 newPos = square.transform.position;
            newPos.y -= moveSpeed * speedMultiplier * Time.deltaTime;  // Applied speed multiplier
            newPos.x = userXOffset;
            square.transform.position = newPos;

            yield return null;
        }

        if (answerMatched)
        {
            resultText.text = "Package Received";
            ColorUtility.TryParseHtmlString("#CCF900", out Color correctColor);
            resultText.color = correctColor;
            audioSource.PlayOneShot(correctSound);
        }
        else
        {
            resultText.text = "Package Missed";
            resultText.color = Color.red;
            SpawnExplosion(square.transform.position);
            audioSource.PlayOneShot(wrongSound);
            yield return new WaitForSeconds(0.5f);
        }

        GenerateNewEquation();
    }

    void SpawnExplosion(Vector3 position)
    {
        GameObject blast = Instantiate(blastPrefab, position, Quaternion.identity);
        Destroy(blast, 1.5f);
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
        StartCoroutine(IncreaseSpeedAfterDelay());
    }

    IEnumerator IncreaseSpeedAfterDelay()
    {
        yield return new WaitForSeconds(2f);  // Wait for 2 seconds
        SetSpeedMultiplier(6f);  // Increase speed multiplier to 3
    }

    // Function to update speed multiplier dynamically
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0.1f, multiplier);  // Ensuring it doesn’t go below 0.1
    }
}
