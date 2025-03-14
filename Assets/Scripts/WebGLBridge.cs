using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

[System.Serializable]
public class UserData
{
    public string userId;
    public string tournamentId;
    public string roundId;
    public bool isTrial;
}

public class WebGLBridge : MonoBehaviour
{
    public static WebGLBridge Instance;
    public GameObject trialGameObject; // ✅ Assign in Inspector (UI for trial mode)
    public GameObject gameStartObject; // ✅ Assign the GameObject containing `GameStartManager`

    public GameStartManager gameStartManager;
    private string baseUrl = "http://localhost:8008/api/v1/webgl-game";
    private UserData userData = new UserData(); // ✅ Centralized user data storage

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("✅ WebGLBridge Initialized");

        // ✅ Get GameStartManager from the assigned GameObject
        if (gameStartObject != null)
        {
            gameStartManager = gameStartObject.GetComponent<GameStartManager>();
            if (gameStartManager == null)
            {
                Debug.LogError("❌ GameStartManager component is missing on the assigned GameObject!");
            }
            else
            {
                Debug.Log("found GameStartManager ");
            }
        }
        else
        {
            Debug.LogError("❌ GameStartObject is not assigned in the Inspector!");
        }
    }

    public void ReceiveDataFromReact(string jsonData)
    {
        Debug.Log("📥 Received from React: " + jsonData);
        try
        {
            userData = JsonUtility.FromJson<UserData>(jsonData);
            Debug.Log($"✅ Stored User Data -> User ID: {userData.userId}, Tournament: {userData.tournamentId}, Round: {userData.roundId}, IsTrial: {userData.isTrial}");

            // ✅ Enable/Disable trialGameObject based on isTrial
            if (trialGameObject != null)
            {
                trialGameObject.SetActive(userData.isTrial);
                Debug.Log($"🎮 Trial Mode: {userData.isTrial} -> trialGameObject {(userData.isTrial ? "ENABLED" : "DISABLED")}");
            }

            // ✅ If NOT a trial game, start the game automatically
            if (!userData.isTrial)
            {
                StartCoroutine(StartGameWithDelay());
                Debug.Log("🚀 Starting main game since it's NOT a trial.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ JSON Parse Error: " + e.Message);
        }
    }
    IEnumerator StartGameWithDelay()
    {
        yield return new WaitForSeconds(0.2f); // Small delay before starting the game

        // 🎯 Simulate the start button click instead of directly calling StartGame()
        if (gameStartManager != null && gameStartManager.startButton1 != null)
        {
            gameStartManager.startButton1.onClick.Invoke();
            Debug.Log("✅ Simulated Start Button Click.");
        }
        else
        {
            Debug.LogError("❌ Start button not found.");
        }
    }



    public void StartGame()
    {
        string startTime = DateTime.UtcNow.ToString("o");

        // 🔄 Manually constructing JSON
        string json = $"{{" +
            $"\"userId\": \"{userData.userId}\", " +
            $"\"tournamentId\": \"{userData.tournamentId}\", " +
            $"\"roundId\": \"{userData.roundId}\", " +
            $"\"startTime\": \"{startTime}\"" +
            $"}}";

        StartCoroutine(SendGameData("start-time", json));
    }

    public void UpdateScore(int score, string jsonData)
    {
        // 🔄 Constructing JSON with properly formatted extra data
        string json = $"{{" +
            $"\"userId\": \"{userData.userId}\", " +
            $"\"tournamentId\": \"{userData.tournamentId}\", " +
            $"\"roundId\": \"{userData.roundId}\", " +
            $"\"score\": {score}, " +
            $"\"attemptedWord\": {jsonData}" +  // ✅ Properly formatted JSON
            $"}}";

        if (!userData.isTrial)
        {
            StartCoroutine(SendGameData("update-score", json));
        }
    }

    public void EndGame()
    {
        string endpoint = userData.isTrial ? "end-trial" : "end-game"; // ✅ Dynamic API selection

        // 🔄 Manually constructing JSON
        string json = $"{{" +
            $"\"userId\": \"{userData.userId}\", " +
            $"\"tournamentId\": \"{userData.tournamentId}\", " +
            $"\"roundId\": \"{userData.roundId}\", " +
            $"\"TrialEnded\": {userData.isTrial.ToString().ToLower()}, " +
            $"\"GameEnded\": {(!userData.isTrial).ToString().ToLower()}" +
            $"}}";

        StartCoroutine(SendGameData(endpoint, json));
    }

    private IEnumerator SendGameData(string action, string json)
    {
        string url = $"{baseUrl}/{action}";
        Debug.Log($"📡 Sending [{action.ToUpper()}] request to: {url}");
        Debug.Log($"📜 Payload: {json}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ [{action.ToUpper()}] API Success: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ [{action.ToUpper()}] API Error: {request.error}");
            }
        }
    }
}
