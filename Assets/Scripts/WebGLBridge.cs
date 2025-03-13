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
    private string baseUrl = "http://localhost:8008/api/v1/webgl-game";
    private UserData userData = new UserData(); // ✅ Centralized user data storage

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Persisting across scenes
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("✅ WebGLBridge Initialized");
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

    public void UpdateScore(int score)
    {
        // 🔄 Manually constructing JSON
        string json = $"{{" +
            $"\"userId\": \"{userData.userId}\", " +
            $"\"tournamentId\": \"{userData.tournamentId}\", " +
            $"\"roundId\": \"{userData.roundId}\", " +
            $"\"score\": {score}" +
            $"}}";
        if(!userData.isTrial){
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
