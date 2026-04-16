using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class CollectSceneUI : MonoBehaviour
{
    // =========================
    // REFERENCES
    // =========================

    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    [SerializeField] private RecognitionService recognitionService;
    [SerializeField] private GestureSender gestureSender;

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text templateNumber;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;

    // =========================
    // SETTINGS
    // =========================

    [SerializeField] private float minimumScore;

    // =========================
    // STATE
    // =========================

    private OneLineDrawable oneLineDrawable;
    private string currentLabel = "free draw";

    public event Action<float> OnDataSent;
    public event Action<float> OnSkip;

    // =========================
    // LIFECYCLE
    // =========================

    private void Awake()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
    }

    private void Start()
    {
        oneLineDrawable = OneLineDrawable.drawable;
    }

    // =========================
    // UI CONTROL
    // =========================

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

    // =========================
    // INPUT
    // =========================

    private void OnNextButtonClicked()
    {
        var points = oneLineDrawable.GetDrawPoints();

        if (points.Length == 0)
            return;

        float scoreValue = ComputeScore(points);

        UpdateScoreUI(scoreValue);

        if (!IsValid(scoreValue))
        {
            PlayFail();
            return;
        }

        PlaySuccess();

        gestureSender.Send(points, currentLabel, recognitionService.GetMode());

        OnDataSent?.Invoke(scoreValue);

        ResetDrawing();
    }

    private void OnSkipButtonClicked()
    {
        ResetDrawing();
        OnSkip?.Invoke(0f);
    }

    // =========================
    // SCORE
    // =========================

    private float ComputeScore(Vector2[] points)
    {
        float rawScore = recognitionService.Evaluate(points, currentLabel);
        return Mathf.Round(rawScore * 100f);
    }

    // =========================
    // UI FEEDBACK
    // =========================

    private void UpdateScoreUI(float value)
    {
        score.text = "Previous score : " + value;
        score.color = value < minimumScore ? Color.red : Color.green;
    }

    private bool IsValid(float value)
    {
        return value >= minimumScore;
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

    private void ResetDrawing()
    {
        oneLineDrawable.ResetCanvas(oneLineDrawable.drawable_texture);
    }
}