using UnityEngine;

public enum RecognitionMode
{
    DollarOne,
    MachineLearning
}

public class RecognitionService : MonoBehaviour
{
    [SerializeField] private UserRecognition userRecognition;
    [SerializeField] private MLModel mlModel;
    [SerializeField] private RecognitionMode mode;

    public float Evaluate(Vector2[] points, string expectedLabel)
    {
        switch (mode)
        {
            case RecognitionMode.DollarOne:
                return userRecognition.OnDrawFinished(points, expectedLabel);

            case RecognitionMode.MachineLearning:
                return EvaluateML(points, expectedLabel);
        }

        return 0f;
    }

    private float EvaluateML(Vector2[] points, string expectedLabel)
    {
        float[] input = PreprocessUtils.ProcessML(points);

        var (predicted, confidence) = mlModel.Predict(input);

        if (predicted != expectedLabel)
            return 0f;

        return confidence;
    }

    public RecognitionMode GetMode()
    {
        return mode;
    }
}