// Assets/Scripts/BardAPIManager.cs

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class BardAPIManager : MonoBehaviour
{
    public static BardAPIManager Instance { get; private set; }
    private BardSessionData sess;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        sess = BardSessionData.FromUrl(Application.absoluteURL);
    }

    // C# osztályok a JSON-parsoláshoz
    [System.Serializable]
    private class PlaySessionData
    {
        public bool canStartGame;
        public string username;
        public bool saveResults;
        public string input;
    }

    [System.Serializable]
    private class PlaySessionResponse
    {
        public PlaySessionData data;
        public string status;
    }

    public IEnumerator CheckCanStart(System.Action<bool> callback)
    {
        string url = $"https://test.bardtest.gg/api/play-sessions/{sess.playSessionUuid}"
                   + $"?game={sess.testName}&input={sess.inputMode}&variant={sess.variant}";
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            // JsonUtility-t használunk
            var resp = JsonUtility.FromJson<PlaySessionResponse>(req.downloadHandler.text);
            callback(resp.data.canStartGame);
        }
        else
        {
            Debug.LogError("API GET error: " + req.error);
            callback(false);
        }
    }

    public IEnumerator SubmitResult(int finalScore, System.Action<bool> callback)
    {
        string url = $"https://test.bardtest.gg/api/results/{sess.playSessionUuid}";
        var body = new
        {
            game    = sess.testName,
            variant = sess.variant,
            input   = sess.inputMode,
            results = new { score = finalScore }
        };
        string jsonBody = JsonUtility.ToJson(body);

        using var req = new UnityWebRequest(url, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        callback(req.result == UnityWebRequest.Result.Success);
    }
}
