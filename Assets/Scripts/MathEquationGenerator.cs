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
    private bool isFirstEquation = true;

    private Vector3 startPosition = new Vector3(0, 2.5f, 0);
    public float endYPosition = -2.5f;
    private float moveSpeed = 0.3f;
    public float speedMultiplier = 1f;

    // **New Variables**
    private float previousLocation;
    private int correctStepsToMove;
    private int stepsMoved;
    private int numberLineLocation;
    public TMP_Text stepsMovedText;
    public int difficultyLevel = 6;

    void Start()
    {
        equationText.text = "";
        answerText.text = "0";
        resultText.text = "";
        audioSource = GetComponent<AudioSource>();

        leftButton.onClick.AddListener(() => OnButtonPressed(-1));
        rightButton.onClick.AddListener(() => OnButtonPressed(1));

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

        if (!float.TryParse(answerText.text, out previousLocation))
        {
            previousLocation = 0f;
        }

        int num1 = 0, num2 = 0, num3 = 0;
        correctAnswer = 0;
        bool isAddition;

        do
        {
            if (difficultyLevel == 4)
            {
                // Single number equation (e.g., "5")
                correctAnswer = UnityEngine.Random.Range(1, 10);
                equationText.text = $"{correctAnswer}";
            }
            else if (difficultyLevel == 6)
            {
                // Two-number equation with random addition or subtraction
                num1 = UnityEngine.Random.Range(1, 10);
                num2 = UnityEngine.Random.Range(1, 10);
                isAddition = UnityEngine.Random.value > 0.5f;

                correctAnswer = isAddition ? num1 + num2 : num1 - num2;
                equationText.text = isAddition ? $"{num1} + {num2} = ?" : $"{num1} - {num2} = ?";
            }
            else if (difficultyLevel == 8)
            {
                // Three-number equation with random operations
                num1 = UnityEngine.Random.Range(1, 5);
                num2 = UnityEngine.Random.Range(1, 5);
                num3 = UnityEngine.Random.Range(1, 5);
                isAddition = UnityEngine.Random.value > 0.5f;

                if (isAddition)
                {
                    correctAnswer = num1 + num2 + num3;
                    equationText.text = $"{num1} + {num2} + {num3} = ?";
                }
                else
                {
                    correctAnswer = num1 + num2 - num3;
                    equationText.text = $"{num1} + {num2} - {num3} = ?";
                }
            }
        }
        while (Mathf.Abs(correctAnswer - previousLocation) < 3); // Ensure difference is at least 3

        correctStepsToMove = correctAnswer - Mathf.RoundToInt(previousLocation);
        answerMatched = false;
        speedIncreased = false;
        GenerateSquare(correctStepsToMove);

        if (isFirstEquation)
        {
            WebGLBridge.Instance.StartGame();
            isFirstEquation = false;
        }
    }

    void LogEquationData()
    {
        string result = answerMatched ? "Correct" : "Incorrect";

        // Updated JSON to include correctAnswer and previousLocation
        string equationJson = $"{{ \"question\":\"{equationText.text.Replace(" = ?", "")}\", " +
            $"\"numberLineLocation\":{numberLineLocation}, " +
            $"\"correctStepsToMove\":{correctStepsToMove}, " +
            $"\"stepsMoved\":{stepsMoved}, " +
            $"\"correctAnswer\":{correctAnswer}, " + // New field added
            $"\"previousLocation\":{previousLocation}, " + // New field added
            $"\"result\":\"{result}\" }}";

        Debug.Log($"📜 Equation JSON: {equationJson}");

        string formattedEquationJson = $"[{equationJson}]";

        WebGLBridge.Instance.UpdateScore(ScoreManager.Instance.GetScore(), formattedEquationJson);
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
            newPos.x = userXOffset/2;
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

    void OnButtonPressed(int direction)
    {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);

        if (!int.TryParse(stepsMovedText.text, out stepsMoved))
        {
            stepsMoved = 0;
        }

        stepsMoved *= direction;

        numberLineLocation = Mathf.RoundToInt(previousLocation + stepsMoved);

        stepsMovedText.text = stepsMoved.ToString();

        Debug.Log(stepsMoved);

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
}
