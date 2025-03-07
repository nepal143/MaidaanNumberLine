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
    public GameObject arrow; // Arrow GameObject reference

    private int correctAnswer;
    private float checkTimer = 0f;
    private string lastCheckedAnswer = "";
    private float checkDelay = 0.2f;
    private GameObject currentSquare;
    private bool answerMatched = false;
    private bool packageSpawned = false;

    private Vector3 startPosition = new Vector3(0, 2.5f, 0);
    public float endYPosition = -2.5f;
    private float moveDuration = 2f; // Falls for 2 seconds
    private float spawnDelay = 2f; // Parcel spawns after 2 seconds

    void Start()
    {
        equationText.text = "";
        answerText.text = "0.00";
        resultText.text = "";

        leftButton.onClick.AddListener(() => OnButtonPressed("left"));
        rightButton.onClick.AddListener(() => OnButtonPressed("right"));

        arrow.SetActive(false); // Hide arrow at start

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

        packageSpawned = false;

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

        answerMatched = false;
    }

    void CheckAnswer()
    {
        if (!answerMatched && float.TryParse(answerText.text, out float userAnswer))
        {
            if (Mathf.RoundToInt(userAnswer) == correctAnswer)
            {
                answerMatched = true;
                resultText.text = "Package Received";
                Debug.Log("Correct Answer! Waiting for animation to finish...");
            }
        }
    }

    void GenerateSquare()
    {
        float spawnXPosition = correctAnswer; // Default X position
        if (float.TryParse(answerText.text, out float userAnswer))
        {
            spawnXPosition -= userAnswer; // Spawn based on user's answer
        }

        Vector3 spawnPosition = new Vector3(spawnXPosition, startPosition.y, startPosition.z);
        currentSquare = Instantiate(squarePrefab, spawnPosition, Quaternion.identity);

        arrow.SetActive(true); // Enable arrow when package starts falling

        StartCoroutine(MoveSquare(currentSquare));
    }

    IEnumerator MoveSquare(GameObject square)
    {
        float elapsedTime = 0f;
        Vector3 startPos = currentSquare.transform.position;
        Vector3 endPos = new Vector3(startPos.x, endYPosition, startPos.z);

        while (elapsedTime < moveDuration)
        {
            square.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        square.transform.position = endPos;

        arrow.SetActive(false); // Disable arrow when package reaches the ground

        if (answerMatched)
        {
            Debug.Log("Package successfully delivered!");
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

        if (!packageSpawned)
        {
            packageSpawned = true;
            StartCoroutine(SpawnPackageAfterDelay());
        }
    }

    IEnumerator SpawnPackageAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        GenerateSquare();
    }
}
