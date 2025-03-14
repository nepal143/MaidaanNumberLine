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

    private bool hasGameStarted = false; // ✅ To prevent multiple calls to StartGame()

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
        }
        catch (Exception e)
        {
            Debug.LogError("❌ JSON Parse Error: " + e.Message);
        }
    }

    void Update()
    {
        if (userData != null)
        {
            // ✅ Enable/Disable Trial UI based on isTrial
            if (trialGameObject != null && trialGameObject.activeSelf != userData.isTrial)
            {
                trialGameObject.SetActive(userData.isTrial);
                Debug.Log($"🔄 Trial Mode Updated: {userData.isTrial} -> trialGameObject {(userData.isTrial ? "ENABLED" : "DISABLED")}");
            }

            // ✅ Automatically start the game only once (if NOT trial)
            if (!userData.isTrial && !hasGameStarted)
            {
                hasGameStarted = true; // Prevent multiple calls
                StartGame();
            }
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
        Debug.Log("🚀 Main Game Started!");
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

        if (!userData.isTrial)
        {
            StartCoroutine(SendGameData("update-score", json));
        }
    }

    public void EndGame()
    {
        string endpoint = userData.isTrial ? "end-trial" : "end-game";

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
