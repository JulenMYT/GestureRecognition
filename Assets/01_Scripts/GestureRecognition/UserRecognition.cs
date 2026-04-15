using UnityEngine;

public class UserRecognition : MonoBehaviour
{
    [SerializeField] private OneLineDrawable _drawable;

    private GestureTemplates _templates => GestureTemplates.Get();
    private static readonly DollarOneRecognizer _dollarOneRecognizer = new DollarOneRecognizer();
    private IRecognizer _currentRecognizer = _dollarOneRecognizer;

    public float OnDrawFinished(DollarPoint[] points, string label = "")
    {
        (string, float) result = _currentRecognizer.DoRecognitionWithLabel(points, 64, _templates.ProceedTemplates, label);
        string resultText = "";
        resultText = $"Recognized: {result.Item1}, Score: {result.Item2}";

        Debug.Log(resultText);
        return result.Item2;
    }
}