// Assets/Scripts/ScoreManager.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;   
    public TMP_Text statusText;  
    public Button addButton, submitButton;
    private int serverScore = 0;

    void Start()
    {
        statusText.text = "Connecting...";
        StartCoroutine(
            BardAPIManager.Instance.CheckCanStart(ok =>
            {
                if (!ok)
                {
                    statusText.text = "Cannot start test.";
                }
                else
                {
                    statusText.text = "Connected";
                    SetupWebSocket();
                }
            })
        );
    }

    void SetupWebSocket()
    {
        // IDEIGENES: helytelen FindObjectOfType helyett használjuk az új API-t
        var wsClient = Object.FindFirstObjectByType<BardWebSocketClient>();

        wsClient.OnValueReceived += v =>
        {
            serverScore = v;
            scoreText.text = "Score: " + serverScore;
        };
        wsClient.OnStatusChanged += s =>
        {
            statusText.text = s;
        };

        addButton.onClick.AddListener(() =>
        {
            wsClient.SendScoreEvent(1);
        });

        submitButton.onClick.AddListener(() =>
        {
            StartCoroutine(
                BardAPIManager.Instance.SubmitResult(serverScore, success =>
                {
                    if (success)
                        Application.OpenURL("https://test.bardtest.gg/progressing-play-session");
                    else
                        statusText.text = "Submit failed.";
                })
            );
        });
    }
}
