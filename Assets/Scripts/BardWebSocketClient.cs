// Assets/Scripts/BardWebSocketClient.cs

using UnityEngine;
using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.Text;

public class BardWebSocketClient : MonoBehaviour
{
    private WebSocket ws;
    private BardSessionData sess;
    private Queue<string> pendingMessages = new Queue<string>();
    
    // Events to notify other scripts of incoming values or status changes
    public event Action<int> OnValueReceived;
    public event Action<string> OnStatusChanged;

    private async void Start()
    {
        // Parse session parameters from URL
        sess = BardSessionData.FromUrl(Application.absoluteURL);
        await Connect();
    }

    private async System.Threading.Tasks.Task Connect()
    {
        string url = "wss://test.bardtest.gg/websocket";
        ws = new WebSocket(url);

        ws.OnOpen += () =>
        {
            Debug.Log("WebSocket opened.");
            OnStatusChanged?.Invoke("Connected");
            // Send any messages that queued up while disconnected
            FlushPending();
        };

        ws.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
            OnStatusChanged?.Invoke("Error");
        };

        ws.OnClose += (e) =>
        {
            Debug.LogWarning("WebSocket closed with code: " + e);
            OnStatusChanged?.Invoke("Disconnected");
            // Attempt reconnect after 5 seconds
            Invoke(nameof(TryReconnect), 5f);
        };

        ws.OnMessage += (bytes) =>
        {
            // convert bytes to string
            var msg = Encoding.UTF8.GetString(bytes);
            HandleMessage(msg);
        };

        // Connect asynchronously
        await ws.Connect();
    }

    private void TryReconnect()
    {
        if (ws.State != WebSocketState.Open)
        {
            Debug.Log("Reconnecting WebSocket...");
            _ = Connect();
        }
    }

    private void HandleMessage(string json)
    {
        try
        {
            var msg = JsonUtility.FromJson<ValueMessage>(json);
            OnValueReceived?.Invoke(msg.value);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse WebSocket message: " + ex);
        }
    }

    private void FlushPending()
    {
        while (pendingMessages.Count > 0 && ws.State == WebSocketState.Open)
        {
            var msg = pendingMessages.Dequeue();
            _ = ws.SendText(msg);
        }
    }

    /// <summary>
    /// Call this to send a score increment event over WebSocket.
    /// If the socket isn't open yet, the message is queued.
    /// </summary>
    public async void SendScoreEvent(int delta)
    {
        var evt = new ScoreEvent
        {
            game = sess.testName,
            data = new[] { new DataPoint { score = delta } }
        };
        string json = JsonUtility.ToJson(evt);

        if (ws.State == WebSocketState.Open)
        {
            await ws.SendText(json);
        }
        else
        {
            pendingMessages.Enqueue(json);
        }
    }



    private void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }

    // --- Helper classes for JSON serialization ---

    [Serializable]
    private class ScoreEvent
    {
        public string game;
        public DataPoint[] data;
    }

    [Serializable]
    private class DataPoint
    {
        public int score;
    }

    [Serializable]
    private class ValueMessage
    {
        public string messageId;
        public int value;
    }
}
