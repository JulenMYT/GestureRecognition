using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class OneLineDrawable : MonoBehaviour
{
    public static Color Pen_Colour = Color.red;
    public static int Pen_Width = 3;

    public LayerMask Drawing_Layers;
    public bool Reset_Canvas_On_Play = true;
    public Color Reset_Colour = new Color(0, 0, 0, 0);

    public static OneLineDrawable drawable;

    public Sprite drawable_sprite;
    public Texture2D drawable_texture;

    Vector2 previous_drag_position;
    Color[] clean_colours_array;
    Color32[] cur_colors;

    bool mouse_was_previously_held_down = false;
    bool no_drawing_on_current_drag = false;

    List<Vector2> _drawPoints = new List<Vector2>();

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        bool inputDown = false;
        Vector2 inputPosition = Vector2.zero;

        if (Input.touchCount > 0)
        {
            inputDown = true;
            inputPosition = Input.GetTouch(0).position;
        }
        else if (Input.GetMouseButton(0))
        {
            inputDown = true;
            inputPosition = Input.mousePosition;
        }

        if (inputDown && !no_drawing_on_current_drag)
        {
            HandleDrawing(inputPosition);
        }
        else
        {
            HandleRelease(inputDown);
        }

        mouse_was_previously_held_down = inputDown;
    }

    void HandleDrawing(Vector2 screenPosition)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        Collider2D hit = Physics2D.OverlapPoint(worldPos, Drawing_Layers.value);

        if (hit != null && hit.transform != null)
        {
            if (previous_drag_position == Vector2.zero)
            {
                ResetCanvas(drawable_texture);
            }

            PenBrush(worldPos);
        }
        else
        {
            previous_drag_position = Vector2.zero;

            if (!mouse_was_previously_held_down)
            {
                no_drawing_on_current_drag = true;
            }
        }
    }

    void HandleRelease(bool mouse_held_down)
    {
        if (!mouse_held_down)
        {
            previous_drag_position = Vector2.zero;
            no_drawing_on_current_drag = false;
        }
    }

    public void PenBrush(Vector2 world_point)
    {
        Vector2 pixel_pos = WorldToPixelCoordinates(world_point);

        cur_colors = drawable_texture.GetPixels32();

        if (previous_drag_position == Vector2.zero)
        {
            MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
        }
        else
        {
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
        }

        _drawPoints.Add(pixel_pos);

        ApplyMarkedPixelChanges(drawable_texture, cur_colors);

        previous_drag_position = pixel_pos;
    }


    public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
    {
        float distance = Vector2.Distance(start_point, end_point);
        Vector2 cur_position = start_point;

        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }
    }

    public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            if (x >= (int)drawable_sprite.rect.width || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                MarkPixelToChange(x, y, color_of_pen, cur_colors);
            }
        }
    }

    public void MarkPixelToChange(int x, int y, Color color, Color32[] textureColors)
    {
        int array_pos = y * (int)drawable_sprite.rect.width + x;

        if (array_pos > textureColors.Length || array_pos < 0)
            return;

        textureColors[array_pos] = color;
    }

    public void ApplyMarkedPixelChanges(Texture2D texture, Color32[] colors)
    {
        texture.SetPixels32(colors);
        texture.Apply(false);
    }


    public Vector2 WorldToPixelCoordinates(Vector2 world_position)
    {
        Vector3 local_pos = transform.InverseTransformPoint(world_position);

        float pixelWidth = drawable_sprite.rect.width;
        float pixelHeight = drawable_sprite.rect.height;

        float unitsToPixels = pixelWidth / drawable_sprite.bounds.size.x * transform.localScale.x;

        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

        return new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));
    }


    public void ResetCanvas(Texture2D texture)
    {
        _drawPoints.Clear();
        texture.SetPixels(clean_colours_array);
        texture.Apply();
    }


    void Awake()
    {
        drawable = this;

        int size = (int)drawable_sprite.rect.width * (int)drawable_sprite.rect.height;
        clean_colours_array = new Color[size];

        for (int i = 0; i < size; i++)
        {
            clean_colours_array[i] = Reset_Colour;
        }

        if (Reset_Canvas_On_Play)
        {
            ResetCanvas(drawable_texture);
        }
    }

    public Vector2[] GetDrawPoints()
    {
        return _drawPoints.ToArray();
    }
}