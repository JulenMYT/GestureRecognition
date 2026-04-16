using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public enum RecognitionMode
{
    DollarOne,
    MachineLearning
}

public class CollectSceneUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private UserRecognition userRecognition;
    [SerializeField] private float minimumScore;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text score;
    [SerializeField] private RecognitionMode mode = RecognitionMode.MachineLearning;
    [SerializeField] private MLModel mlModel;

    [SerializeField] private TMP_Text templateNumber;

    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;

    private OneLineDrawable oneLineDrawable;

    [SerializeField] private CanvasGroup canvasGroup;

    const string APP_VERSION = "1.2";

    private string currentLabel = "free draw";

    public event Action<float> OnDataSent;
    public event Action<float> OnSkip;

    private void Awake()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
    }

    private void Start()
    {
        oneLineDrawable = OneLineDrawable.drawable;
    }

    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void SetCurrentSymbol(Template template)
    {
        currentLabel = template.label;
        image.sprite = template.sprite;
    }

    public void SetTemplateNumber(int current, int total)
    {
        templateNumber.text = $"Template {current} / {total}";
    }

    private void OnNextButtonClicked()
    {
        var points = oneLineDrawable.GetDrawPoints();
        if (points.Length == 0)
            return;

        float result = Evaluate(points);
        result *= 100;
        result = Mathf.Round(result);

        UpdateScoreUI(result);

        if (!IsValid(result))
        {
            PlayFail();
            return;
        }

        PlaySuccess();

        SendData(points);
        OnDataSent?.Invoke(result);
        ResetDrawing();
    }

    private void OnSkipButtonClicked()
    {
        ResetDrawing();
        OnSkip?.Invoke(0.0f);
    }

    private float Evaluate(Vector2[] points)
    {
        switch (mode)
        {
            case RecognitionMode.DollarOne:
                return userRecognition.OnDrawFinished(points, currentLabel);

            case RecognitionMode.MachineLearning:
                float[] input = Preprocess.Process(ConvertPoints(points));

                var (predicted, confidence) = mlModel.Predict(input);

                Debug.Log("ML Predicted: " + predicted + " (" + confidence + ")");

                if (predicted != currentLabel)
                    return 0f;

                return confidence;
        }

        return 0f;
    }

    private void UpdateScoreUI(float value)
    {
        score.text = "Previous score : " + value;

        if (value < minimumScore)
            score.color = Color.red;
        else
            score.color = Color.green;
    }

    private bool IsValid(float value)
    {
        return value >= minimumScore;
    }

    private void SendData(Vector2[] points)
    {
        GestureData data = new GestureData();

        data.label = currentLabel;
        data.points = ConvertPoints(points);
        data.numPoints = data.points.Length;

        data.userId = SystemInfo.deviceUniqueIdentifier;
        data.version = APP_VERSION;
        data.createdAt = DateTime.UtcNow.ToString("o");
        data.mode = mode.ToString();

        string json = JsonUtility.ToJson(data);

        apiClient.SendGesture(json);
    }

    private void ResetDrawing()
    {
        oneLineDrawable.ResetCanvas(oneLineDrawable.drawable_texture);
    }

    private void PlaySuccess()
    {
        if (successClip != null)
            AudioManager.Instance.PlaySound(successClip, 1f, false);
    }

    private void PlayFail()
    {
        if (failClip != null)
            AudioManager.Instance.PlaySound(failClip, 1f, false);
    }

    private Vector2[] ConvertPoints(Vector2[] points)
    {
        Vector2[] result = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            result[i] = points[i];
        }

        return result;
    }
}