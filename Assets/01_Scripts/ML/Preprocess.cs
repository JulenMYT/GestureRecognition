using UnityEngine;
using System.Collections.Generic;

public static class Preprocess
{
    public static float[] Process(Vector2[] points)
    {
        var pts = ToList(points);

        pts = Resample(pts, 64);
        pts = TranslateToOrigin(pts);
        pts = Scale(pts);

        return Flatten(pts);
    }

    private static List<Vector2> ToList(Vector2[] points)
    {
        return new List<Vector2>(points);
    }

    private static List<Vector2> Resample(List<Vector2> points, int n)
    {
        float pathLength = 0f;

        for (int i = 1; i < points.Count; i++)
            pathLength += Vector2.Distance(points[i - 1], points[i]);

        float step = pathLength / (n - 1);

        List<Vector2> newPoints = new List<Vector2>();
        newPoints.Add(points[0]);

        float distAccum = 0f;

        int iIndex = 1;
        Vector2 prev = points[0];

        while (iIndex < points.Count)
        {
            Vector2 curr = points[iIndex];
            float d = Vector2.Distance(prev, curr);

            if (distAccum + d >= step)
            {
                float t = (step - distAccum) / d;
                Vector2 newPoint = prev + t * (curr - prev);

                newPoints.Add(newPoint);
                prev = newPoint;
                distAccum = 0f;
            }
            else
            {
                distAccum += d;
                prev = curr;
                iIndex++;
            }
        }

        while (newPoints.Count < n)
            newPoints.Add(newPoints[newPoints.Count - 1]);

        if (newPoints.Count > n)
            newPoints.RemoveRange(n, newPoints.Count - n);

        return newPoints;
    }

    private static List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        Vector2 centroid = Vector2.zero;

        foreach (var p in points)
            centroid += p;

        centroid /= points.Count;

        List<Vector2> result = new List<Vector2>();

        foreach (var p in points)
            result.Add(p - centroid);

        return result;
    }

    private static List<Vector2> Scale(List<Vector2> points)
    {
        float max = 0f;

        foreach (var p in points)
        {
            max = Mathf.Max(max, Mathf.Abs(p.x));
            max = Mathf.Max(max, Mathf.Abs(p.y));
        }

        if (max == 0f)
            return points;

        List<Vector2> result = new List<Vector2>();

        foreach (var p in points)
            result.Add(p / max);

        return result;
    }

    private static float[] Flatten(List<Vector2> points)
    {
        float[] result = new float[points.Count * 2];

        for (int i = 0; i < points.Count; i++)
        {
            result[i * 2] = points[i].x;
            result[i * 2 + 1] = points[i].y;
        }

        return result;
    }
}