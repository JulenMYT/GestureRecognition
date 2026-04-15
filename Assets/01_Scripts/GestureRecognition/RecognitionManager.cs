using System;
using System.Linq;
using FreeDraw;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecognitionManager : MonoBehaviour
{
    [SerializeField] private Drawable _drawable;
    [SerializeField] private TextMeshProUGUI _recognitionResult;
    [SerializeField] private Button _templateModeButton;
    [SerializeField] private Button _recognitionModeButton;
    [SerializeField] private Button _reviewTemplates;
    [SerializeField] private TMP_InputField _templateName;
    [SerializeField] private TemplateReviewPanel _templateReviewPanel;

    private GestureTemplates _templates => GestureTemplates.Get();
    private static readonly DollarOneRecognizer _dollarOneRecognizer = new DollarOneRecognizer();
    private IRecognizer _currentRecognizer = _dollarOneRecognizer;
    private RecognizerState _state = RecognizerState.RECOGNITION;

    public enum RecognizerState
    {
        TEMPLATE,
        RECOGNITION,
        TEMPLATE_REVIEW
    }



    private string TemplateName => _templateName.text;

    private void Start()
    {
        _drawable.OnDrawFinished += OnDrawFinished;
        _templateModeButton.onClick.AddListener(() => SetupState(RecognizerState.TEMPLATE));
        _recognitionModeButton.onClick.AddListener(() => SetupState(RecognizerState.RECOGNITION));
        _reviewTemplates.onClick.AddListener(() => SetupState(RecognizerState.TEMPLATE_REVIEW));

        SetupState(_state);
    }

    private void SetupState(RecognizerState state)
    {
        _state = state;
        _templateModeButton.image.color = _state == RecognizerState.TEMPLATE ? Color.green : Color.white;
        _recognitionModeButton.image.color = _state == RecognizerState.RECOGNITION ? Color.green : Color.white;
        _reviewTemplates.image.color = _state == RecognizerState.TEMPLATE_REVIEW ? Color.green : Color.white;

        _templateName.gameObject.SetActive(_state == RecognizerState.TEMPLATE);

        _recognitionResult?.gameObject.SetActive(_state == RecognizerState.RECOGNITION);

        _drawable.gameObject.SetActive(state != RecognizerState.TEMPLATE_REVIEW);
        _templateReviewPanel?.SetVisibility(state == RecognizerState.TEMPLATE_REVIEW);
    }

    private void OnDrawFinished(DollarPoint[] points)
    {
        if (_state == RecognizerState.TEMPLATE)
        {
            GestureTemplate preparedTemplate =
                new GestureTemplate(TemplateName, _currentRecognizer.Normalize(points, 64));
            _templates.RawTemplates.Add(new GestureTemplate(TemplateName, points));
            _templates.ProceedTemplates.Add(preparedTemplate);
        }
        else
        {
            (string, float) result = _currentRecognizer.DoRecognition(points, 64, _templates.ProceedTemplates);
            string resultText = "";
            resultText = $"Recognized: {result.Item1}, Score: {result.Item2}";

            _recognitionResult.text = resultText;
        }
    }

    private void OnApplicationQuit()
    {
        _templates.Save();
    }
}