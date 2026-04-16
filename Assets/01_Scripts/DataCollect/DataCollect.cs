using UnityEngine;

public class DataCollect : MonoBehaviour
{
    [SerializeField] private CollectSceneUI collectSceneUI;
    [SerializeField] private DisplayScoreUI displayScoreUI;
    [SerializeField] private GameObject drawImage;

    [SerializeField] private float minimumAverageScore = 80f;

    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;

    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failSprite;

    private float totalScore = 0f;

    private void Start()
    {
        collectSceneUI.OnDataSent += ReceiveData;
        collectSceneUI.OnSkip += ReceiveData;

        displayScoreUI.OnButtonClickedEvent += Reset;

        StartRun();
    }

    private void StartRun()
    {
        totalScore = 0f;
        ShowNextSymbol();
    }

    private void ReceiveData(float score)
    {
        totalScore += score;
        ShowNextSymbol();
    }

    private void ShowNextSymbol()
    {
        if (IsRunFinished())
        {
            EndRun();
            return;
        }

        UpdateProgressUI();
        LoadNextTemplate();
    }

    private bool IsRunFinished()
    {
        return TemplateCollectionExtensions.GetRemainingRandomTemplates() == 0;
    }

    private void EndRun()
    {
        float averageScore = CalculateAverageScore();

        displayScoreUI.UpdateScore(averageScore);
        displayScoreUI.Show();

        collectSceneUI.Hide();
        drawImage.SetActive(false);

        ApplyResult(averageScore);
    }

    private void ApplyResult(float averageScore)
    {
        bool success = averageScore >= minimumAverageScore;

        displayScoreUI.SetResultImage(success ? successSprite : failSprite);

        PlayResultSound(success);
    }

    private void PlayResultSound(bool success)
    {
        AudioClip clip = success ? successClip : failClip;

        if (clip != null)
            AudioManager.Instance.PlaySound(clip, 1f, false);
    }

    private void UpdateProgressUI()
    {
        int remaining = TemplateCollectionExtensions.GetRemainingRandomTemplates();
        int total = TemplateCollectionExtensions.GetTemplateCount();

        collectSceneUI.SetTemplateNumber(total - remaining + 1, total);
    }

    private void LoadNextTemplate()
    {
        Template template = TemplateCollectionExtensions.GetRandomTemplate();
        collectSceneUI.SetCurrentSymbol(template);
    }

    private float CalculateAverageScore()
    {
        int total = TemplateCollectionExtensions.GetTemplateCount();
        return totalScore / total;
    }

    private void Reset()
    {
        displayScoreUI.Hide();
        collectSceneUI.Show();
        drawImage.SetActive(true);
        TemplateCollectionExtensions.Reset();

        StartRun();
    }
}