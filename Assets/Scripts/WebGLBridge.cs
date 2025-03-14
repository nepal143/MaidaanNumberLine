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
    public string isTrial; // ✅ Kept as a string to check for empty values
}

public class WebGLBridge : MonoBehaviour
{
    public static WebGLBridge Instance;
    public GameObject trialGameObject; // ✅ Assign in Inspector (UI for trial mode)
    public GameObject gameStartObject; // ✅ Assign the GameObject containing `GameStartManager`
    
    public GameStartManager gameStartManager;
    private string baseUrl = "http://localhost:8008/api/v1/webgl-game";
    private UserData userData = new UserData(); // ✅ Centralized user data storage
    private bool receivedData = false; // ✅ Flag to track data reception

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
        }
        else
        {
            Debug.LogError("❌ GameStartObject is not assigned in the Inspector!");
        }

        // ✅ Wait for user data before starting the game
        StartCoroutine(WaitForUserDataAndStartGame());
    }

    public void ReceiveDataFromReact(string jsonData)
    {
        Debug.Log("📥 Received from React: " + jsonData);
        try
        {
            userData = JsonUtility.FromJson<UserData>(jsonData);
            Debug.Log($"✅ Stored User Data -> User ID: {userData.userId}, Tournament: {userData.tournamentId}, Round: {userData.roundId}, IsTrial: {userData.isTrial}");

            if (string.IsNullOrEmpty(userData.isTrial))
            {
                Debug.Log("⚠️ isTrial is empty. Waiting for a valid value...");
                return;
            }

            bool isTrialMode = userData.isTrial.ToLower() == "true";

            // ✅ Enable/Disable trialGameObject based on isTrial
            if (trialGameObject != null)
            {
                trialGameObject.SetActive(isTrialMode);
                Debug.Log($"🎮 Trial Mode: {isTrialMode} -> trialGameObject {(isTrialMode ? "ENABLED" : "DISABLED")}");
            }

            receivedData = true; // ✅ Mark that data has been received
        }
        catch (Exception e)
        {
            Debug.LogError("❌ JSON Parse Error: " + e.Message);
        }
    }

    private IEnumerator WaitForUserDataAndStartGame()
    {
        Debug.Log("⏳ Waiting for user data from React...");
        
        yield return new WaitUntil(() => receivedData); // ✅ Wait until data is received

        if (userData.isTrial.ToLower() != "true")
        {
            Debug.Log("🚀 Starting main game since it's NOT a trial.");
            StartCoroutine(StartGameWithDelay());
        }
        else
        {
            Debug.Log("🛑 Trial mode detected. Not starting the game.");
        }
    }

    IEnumerator StartGameWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        StartGame();
    }

    public void StartGame()
    {
        string startTime = DateTime.UtcNow.ToString("o");

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
        string json = $"{{" +
            $"\"userId\": \"{userData.userId}\", " +
            $"\"tournamentId\": \"{userData.tournamentId}\", " +
            $"\"roundId\": \"{userData.roundId}\", " +
            $"\"score\": {score}, " +
            $"\"attemptedWord\": {jsonData}" +
            $"}}";

        if (userData.isTrial.ToLower() != "true")
        {
            StartCoroutine(SendGameData("update-score", json));
        }
    }

    public void EndGame()
    {
        bool isTrialMode = userData.isTrial.ToLower() == "true";
        string endpoint = isTrialMode ? "end-trial" : "end-game";

        string json = $"{{" +
            $"\"userId\": \"{userData.userId}\", " +
            $"\"tournamentId\": \"{userData.tournamentId}\", " +
            $"\"roundId\": \"{userData.roundId}\", " +
            $"\"TrialEnded\": {isTrialMode.ToString().ToLower()}, " +
            $"\"GameEnded\": {(!isTrialMode).ToString().ToLower()}" +
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
