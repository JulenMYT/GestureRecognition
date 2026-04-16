using System;
using System.Collections.Generic;
using UnityEngine;

public static class PreprocessUtils
{
    // =========================
    // ML PIPELINE (LOCKED)
    // =========================

    // WARNING: Changing this pipeline requires retraining the ML model
    public static float[] ProcessML(Vector2[] points)
    {
        if (points == null || points.Length < 2)
            return new float[128];

        points = Resample(points, 64);
        points = TranslateToOrigin(points);
        points = Normalize(points);

        return Flatten(points);
    }

    // =========================
    // NORMALIZATION (ML)
    // =========================

    private static Vector2[] Normalize(Vector2[] points)
    {
        float max = 0f;

        for (int i = 0; i < points.Length; i++)
        {
            float ax = Mathf.Abs(points[i].x);
            float ay = Mathf.Abs(points[i].y);

            if (ax > max) max = ax;
            if (ay > max) max = ay;
        }

        if (max < 1e-6f)
            return points;

        Vector2[] result = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
            result[i] = points[i] / max;

        return result;
    }

    public static Vector2[] TranslateToOrigin(Vector2[] points)
    {
        Vector2 centroid = GetCentroid(points);

        Vector2[] result = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
            result[i] = points[i] - centroid;

        return result;
    }

    public static Vector2 GetCentroid(Vector2[] points)
    {
        float sumX = 0f;
        float sumY = 0f;

        for (int i = 0; i < points.Length; i++)
        {
            sumX += points[i].x;
            sumY += points[i].y;
        }

        return new Vector2(sumX / points.Length, sumY / points.Length);
    }

    // =========================
    // RESAMPLING (SHARED)
    // =========================

    public static Vector2[] Resample(Vector2[] points, int n)
    {
        float pathLength = PathLength(points);

        if (pathLength < 1e-6f)
        {
            Vector2[] flat = new Vector2[n];
            for (int i = 0; i < n; i++)
                flat[i] = points[0];
            return flat;
        }

        float increment = pathLength / (n - 1);
        float distAccum = 0f;

        List<Vector2> newPoints = new List<Vector2>(n);
        newPoints.Add(points[0]);

        for (int i = 1; i < points.Length; i++)
        {
            Vector2 prev = points[i - 1];
            Vector2 curr = points[i];

            float d = Vector2.Distance(prev, curr);

            if (d < 1e-6f)
                continue;

            while (distAccum + d >= increment && newPoints.Count < n)
            {
                float t = (increment - distAccum) / d;
                t = Mathf.Clamp01(t);

                Vector2 newPoint = prev + t * (curr - prev);
                newPoints.Add(newPoint);

                prev = newPoint;
                d = Vector2.Distance(prev, curr);
                distAccum = 0f;
            }

            distAccum += d;
        }

        while (newPoints.Count < n)
            newPoints.Add(newPoints[newPoints.Count - 1]);

        return newPoints.ToArray();
    }

    public static float PathLength(Vector2[] points)
    {
        float length = 0f;

        for (int i = 1; i < points.Length; i++)
        {
            float d = Vector2.Distance(points[i - 1], points[i]);
            if (!float.IsNaN(d))
                length += d;
        }

        return length;
    }

    // =========================
    // ROTATION ($1 ONLY)
    // =========================

    public static Vector2[] RotateToZero(Vector2[] points)
    {
        float angle = IndicativeAngle(points);
        return RotateBy(points, -angle).ToArray();
    }

    public static List<Vector2> RotateBy(Vector2[] points, float angle)
    {
        List<Vector2> newPoints = new List<Vector2>(points.Length);
        Vector2 centroid = GetCentroid(points);

        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 p = points[i];

            float x = (p.x - centroid.x) * cos - (p.y - centroid.y) * sin + centroid.x;
            float y = (p.x - centroid.x) * sin + (p.y - centroid.y) * cos + centroid.y;

            newPoints.Add(new Vector2(x, y));
        }

        return newPoints;
    }

    public static float IndicativeAngle(Vector2[] points)
    {
        Vector2 centroid = GetCentroid(points);
        return Mathf.Atan2(points[0].y - centroid.y, points[0].x - centroid.x);
    }

    // =========================
    // SCALING ($1 ONLY)
    // =========================

    public static Vector2[] ScaleToSquare(Vector2[] points, float size)
    {
        Rect box = GetBoundingBox(points);

        float width = Mathf.Max(box.width, 1e-6f);
        float height = Mathf.Max(box.height, 1e-6f);

        Vector2[] result = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            result[i] = new Vector2(
                points[i].x * size / width,
                points[i].y * size / height
            );
        }

        return result;
    }

    public static Rect GetBoundingBox(Vector2[] points)
    {
        float minX = points[0].x;
        float maxX = points[0].x;
        float minY = points[0].y;
        float maxY = points[0].y;

        for (int i = 1; i < points.Length; i++)
        {
            Vector2 p = points[i];

            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    // =========================
    // OUTPUT
    // =========================

    private static float[] Flatten(Vector2[] points)
    {
        float[] result = new float[points.Length * 2];

        for (int i = 0; i < points.Length; i++)
        {
            result[i * 2] = points[i].x;
            result[i * 2 + 1] = points[i].y;
        }

        return result;
    }
}