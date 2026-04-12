using System.Linq;
using UnityEngine;

public class DisplayUserData : MonoBehaviour
{
    [SerializeField] private Texture2D drawable_texture;

    private Color32[] cur_colors;

    public void Draw(GestureData gestureData)
    {
        Clear();
        cur_colors = drawable_texture.GetPixels32();
        Vector2[] points = gestureData.points;

        for (int i = 1; i < points.Length; i++)
        {
            Vector2 previous = points[i - 1];
            Vector2 current = points[i];


            ColourBetween(previous, current, 2, Color.red);
        }

        ApplyMarkedPixelChanges(drawable_texture, cur_colors);
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
