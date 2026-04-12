using System;
using UnityEngine;
using UnityEngine.UI;

public class ReviewDataUI : MonoBehaviour
{
    [SerializeField] private Button previous;
    [SerializeField] private Button next;

    [SerializeField] private ApiClient apiClient;
    [SerializeField] private DisplayUserData display;

    GestureData[] gestures;
    int index = 0;
    int page = 0;

    bool goToLastOnLoad = false;

    private void Awake()
    {
        previous.onClick.AddListener(OnPreviousClicked);
        next.onClick.AddListener(OnNextClicked);
    }

    private void Start()
    {
        apiClient.OnGesturesReceived += OnData;
        apiClient.GetGestures(page);
    }

    void OnData(GestureData[] data)
    {
        gestures = data;

        if (goToLastOnLoad)
        {
            index = gestures.Length - 1;
            goToLastOnLoad = false;
        }
        else
        {
            index = 0;
        }

        Show();
    }

    private void OnPreviousClicked()
    {
        if (gestures == null || gestures.Length == 0) return;

        index--;

        if (index < 0)
        {
            if (page > 0)
            {
                page--;
                goToLastOnLoad = true;
                apiClient.GetGestures(page);
                return;
            }

            index = 0;
        }

        Show();
    }

    private void OnNextClicked()
    {
        if (gestures == null || gestures.Length == 0) return;

        index++;

        if (index >= gestures.Length)
        {
            page++;
            apiClient.GetGestures(page);
            return;
        }

        Show();
    }

    void Show()
    {
        if (gestures == null || gestures.Length == 0 || index < 0 || index >= gestures.Length)
        {
            Debug.LogWarning($"[Review] Invalid state Page={page} Index={index}");
            return;
        }

        var g = gestures[index];

        string user = string.IsNullOrEmpty(g.userId) ? "unknown" : g.userId;
        string version = string.IsNullOrEmpty(g.version) ? "unknown" : g.version;

        string formattedDate = "invalid";
        if (!string.IsNullOrEmpty(g.createdAt))
        {
            DateTime date;
            if (DateTime.TryParse(g.createdAt, out date))
            {
                formattedDate = date.ToLocalTime().ToString("dd/MM HH:mm");
            }
        }

        Debug.Log($"[Review] Page={page} Index={index}/{gestures.Length} Label={g.label} Points={g.numPoints} User={user} Version={version} Date={formattedDate}");

        display.Draw(g);
    }
}