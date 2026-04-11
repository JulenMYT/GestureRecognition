using System.Linq;
using UnityEngine;

public class DisplayTemplate : MonoBehaviour
{
    [SerializeField] private Texture2D drawable_texture;
    [SerializeField] private bool _preparedPoints;

    [SerializeField] private bool showPoints;
    [SerializeField] private bool showBoundingBox;
    [SerializeField] private bool showCentroid;

    private Color32[] cur_colors;
    private readonly DollarOneRecognizer _dollarOneRecognizer = new DollarOneRecognizer();

    public void Draw(RecognitionManager.GestureTemplate gestureTemplate, DollarOneRecognizer.Step step)
    {
        Clear();
        cur_colors = drawable_texture.GetPixels32();

        DollarPoint[] points = gestureTemplate.Points.Distinct().ToArray();
        if (step != DollarOneRecognizer.Step.RAW)
            points = _dollarOneRecognizer.Normalize(gestureTemplate.Points, 64, step);

        float xMin = points.Min(p => p.Point.x);
        float xMax = points.Max(p => p.Point.x);
        float yMin = points.Min(p => p.Point.y);
        float yMax = points.Max(p => p.Point.y);

        Vector2 Remap(Vector2 p)
        {
            return new Vector2(
                p.x.Remap(xMin, 0, xMax, xMax - xMin),
                p.y.Remap(yMin, 0, yMax, yMax - yMin)
            );
        }

        for (int i = 1; i < points.Length; i++)
        {
            Vector2 previous = points[i - 1].Point;
            Vector2 current = points[i].Point;

            if (step == DollarOneRecognizer.Step.TRANSLATED)
            {
                previous = Remap(previous);
                current = Remap(current);
            }

            ColourBetween(previous, current, 2, Color.red);
        }

        if (showPoints)
        {
            foreach (var p in points)
            {
                Vector2 pos = p.Point;
                if (step == DollarOneRecognizer.Step.TRANSLATED)
                    pos = Remap(pos);

                MarkPixelsToColour(pos, 2, Color.black);
            }
        }

        if (showBoundingBox)
        {
            Vector2 min = new Vector2(xMin, yMin);
            Vector2 max = new Vector2(xMax, yMax);

            if (step == DollarOneRecognizer.Step.TRANSLATED)
            {
                min = Remap(min);
                max = Remap(max);
            }

            DrawBoundingBox(min.x, min.y, max.x, max.y);
        }

        if (showCentroid)
        {
            Vector2 c = GetCentroid(points);
            if (step == DollarOneRecognizer.Step.TRANSLATED)
                c = Remap(c);

            MarkPixelsToColour(c, 4, Color.blue);
        }

        ApplyMarkedPixelChanges(drawable_texture, cur_colors);
    }

    private Vector2 GetCentroid(DollarPoint[] points)
    {
        float x = points.Average(p => p.Point.x);
        float y = points.Average(p => p.Point.y);
        return new Vector2(x, y);
    }

    private void DrawBoundingBox(float xmin, float ymin, float xmax, float ymax)
    {
        ColourBetween(new Vector2(xmin, ymin), new Vector2(xmax, ymin), 2, Color.green);
        ColourBetween(new Vector2(xmax, ymin), new Vector2(xmax, ymax), 2, Color.green);
        ColourBetween(new Vector2(xmax, ymax), new Vector2(xmin, ymax), 2, Color.green);
        ColourBetween(new Vector2(xmin, ymax), new Vector2(xmin, ymin), 2, Color.green);
    }

    public void Clear()
    {
        Color[] clean_colours_array = new Color[(int)drawable_texture.width * (int)drawable_texture.height];
        for (int x = 0; x < clean_colours_array.Length; x++)
            clean_colours_array[x] = Color.white;

        drawable_texture.SetPixels(clean_colours_array);
        drawable_texture.Apply();
    }

    public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
    {
        float distance = Vector2.Distance(start_point, end_point);
        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            Vector2 cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }
    }

    public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            if (x >= (int)drawable_texture.width || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                MarkPixelToChange(x, y, color_of_pen, cur_colors);
            }
        }
    }

    public void MarkPixelToChange(int x, int y, Color color, Color32[] textureColors)
    {
        int array_pos = y * (int)drawable_texture.width + x;

        if (array_pos > textureColors.Length || array_pos < 0)
            return;

        textureColors[array_pos] = color;
    }

    public void ApplyMarkedPixelChanges(Texture2D texture, Color32[] colors)
    {
        texture.SetPixels32(colors);
        texture.Apply(false);
    }
}