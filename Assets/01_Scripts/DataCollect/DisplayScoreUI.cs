using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayScoreUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup scoreCanvasGroup;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button button;
    [SerializeField] private Image resultImage;

    public event Action OnButtonClickedEvent;

    private void Awake()
    {
        Hide();
        button.onClick.AddListener(OnButtonClicked);
    }

    public void Show()
    {
        scoreCanvasGroup.alpha = 1f;
        scoreCanvasGroup.interactable = true;
        scoreCanvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        scoreCanvasGroup.alpha = 0f;
        scoreCanvasGroup.interactable = false;
        scoreCanvasGroup.blocksRaycasts = false;
    }

    public void UpdateScore(float score)
    {
        scoreText.text = score.ToString("F2");
    }

    public void SetResultImage(Sprite sprite)
    {
        resultImage.sprite = sprite;
    }

    private void OnButtonClicked()
    {
        OnButtonClickedEvent?.Invoke();
    }
}
