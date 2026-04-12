using UnityEngine;
using UnityEngine.UI;

public class CollectSceneUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    private OneLineDrawable oneLineDrawable;
    [SerializeField] private ApiClient apiClient;

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

        GestureData data = new GestureData();
        data.label = "free draw";
        if (points.Length == 0) return;
        data.points = ConvertPoints(points);
        data.numPoints = data.points.Length;

        string json = JsonUtility.ToJson(data);

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
