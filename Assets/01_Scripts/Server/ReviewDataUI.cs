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
        index = 0;
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
        display.Draw(gestures[index]);
    }
}