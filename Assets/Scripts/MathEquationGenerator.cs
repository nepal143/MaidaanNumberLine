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
    private bool speedIncreased = false;

    private Vector3 startPosition = new Vector3(0, 2.5f, 0);
    public float endYPosition = -2.5f;

    private float moveSpeed = 0.3f;
    public float speedMultiplier = 1f;

    void Start()
    {
        equationText.text = "";
        answerText.text = "0.00";
        resultText.text = "";
        audioSource = GetComponent<AudioSource>();

        leftButton.onClick.AddListener(() => OnButtonPressed());
        rightButton.onClick.AddListener(() => OnButtonPressed());

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

        if (speedIncreased)
        {
            SetSpeedMultiplier(7f);
            speedIncreased = false;
        }
    }

    void GenerateNewEquation()
    {
        ResetSpeedMultiplier();
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
        speedIncreased = false;
        GenerateSquare(correctAnswer - previousAnswer);
    }

    void ResetSpeedMultiplier()
    {
        speedMultiplier = 1f;
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
            newPos.y -= moveSpeed * speedMultiplier * Time.deltaTime;
            newPos.x = userXOffset;
            square.transform.position = newPos;

            yield return null;
        }

        if (answerMatched)
        {
            resultText.text = "Package Received";
            resultText.color = new Color(0.8f, 0.98f, 0f);
            audioSource.PlayOneShot(correctSound);
            ScoreManager.Instance.IncreaseScoreOnPackageReceived();
            LogEquationData();
        }
        else
        {
            resultText.text = "Package Missed";
            resultText.color = Color.red;
            SpawnExplosion(square.transform.position);
            audioSource.PlayOneShot(wrongSound);
            LogEquationData();
            yield return new WaitForSeconds(0.5f);
        }

        // **Logging the JSON object**


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

    void OnButtonPressed()
    {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
        StartCoroutine(IncreaseSpeedAfterDelay(currentSquare));
    }

    IEnumerator IncreaseSpeedAfterDelay(GameObject packageAtButtonPress)
    {
        yield return new WaitForSeconds(2.2f);

        if (currentSquare == packageAtButtonPress)
        {
            speedIncreased = true;
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    void LogEquationData()
    {
        // Store the previous number line location before any changes
        float previousLocation;
        if (!float.TryParse(answerText.text, out previousLocation))
        {
            previousLocation = 0; // Default to 0 if parsing fails
        }

        // User's answer (steps moved)
        int stepsMoved;
        if (!int.TryParse(answerText.text, out stepsMoved))
        {
            stepsMoved = 0; // Default to 0 if parsing fails
        }

        int correctStepsToMove = correctAnswer - Mathf.RoundToInt(previousLocation);
        int numberLineLocation = Mathf.RoundToInt(previousLocation + stepsMoved);
        string result = answerMatched ? "Correct" : "Incorrect";

        string json = $"{{ \"question\":\"{equationText.text.Replace(" = ?", "")}\", \"numberLineLocation\":{numberLineLocation}, \"correctStepsToMove\":{correctStepsToMove}, \"stepsMoved\":{stepsMoved}, \"result\":\"{result}\" }}";

        Debug.Log(json);
    }


}
