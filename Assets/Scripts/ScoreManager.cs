using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    private int score = 0; // Ensure score starts at 0
    public TMP_Text scoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreText(); // Ensures score starts as "Score: 0"
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
        Debug.Log("Score Increased: " + amount + " | Total Score: " + score);
        UpdateScoreText();
    }

    public int GetScore()
    {
        return score;
    }

    public void IncreaseScoreOnPackageReceived()
    {
        IncreaseScore(2);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogWarning("Score Text reference is missing!");
        }
    }
}
