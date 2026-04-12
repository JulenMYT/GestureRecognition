using UnityEngine;
using UnityEngine.UI;
using System;

public class CollectSceneUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    private OneLineDrawable oneLineDrawable;
    [SerializeField] private ApiClient apiClient;

    const string APP_VERSION = "1.1";

    private void Awake()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void Start()
    {
        oneLineDrawable = OneLineDrawable.drawable;
    }

    private void OnNextButtonClicked()
    {
        var points = oneLineDrawable.GetDrawPoints();

        if (points.Length == 0) return;

        GestureData data = new GestureData();

        data.label = "free draw";
        data.points = ConvertPoints(points);
        data.numPoints = data.points.Length;

        data.userId = SystemInfo.deviceUniqueIdentifier;
        data.version = APP_VERSION;
        data.createdAt = DateTime.UtcNow.ToString("o");

        string json = JsonUtility.ToJson(data);

        Debug.Log($"[SEND] User={data.userId} Version={data.version} Points={data.numPoints}");

        apiClient.SendGesture(json);

        oneLineDrawable.ResetCanvas(oneLineDrawable.drawable_texture);
    }

    Vector2[] ConvertPoints(DollarPoint[] points)
    {
        Vector2[] result = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            result[i] = points[i].Point;
        }

        return result;
    }
}