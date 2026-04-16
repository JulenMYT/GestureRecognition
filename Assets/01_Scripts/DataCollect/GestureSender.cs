using UnityEngine;
using System;

public class GestureSender : MonoBehaviour
{
    [SerializeField] private ApiClient apiClient;

    private const string APP_VERSION = "1.2";

    public void Send(Vector2[] points, string label, RecognitionMode mode)
    {
        GestureData data = new GestureData
        {
            label = label,
            points = points,
            numPoints = points.Length,
            userId = SystemInfo.deviceUniqueIdentifier,
            version = APP_VERSION,
            createdAt = DateTime.UtcNow.ToString("o"),
            mode = mode.ToString()
        };

        string json = JsonUtility.ToJson(data);
        apiClient.SendGesture(json);
    }
}