using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DollarOneRecognizer
{
    private const float Theta = 45f;
    private const float DeltaTheta = 2f;
    private static readonly float GoldenRatio = 0.5f * (-1 + Mathf.Sqrt(5));

    private int _size = 250;

    public enum Step
    {
        RAW,
        RESAMPLED,
        ROTATED,
        SCALED,
        TRANSLATED
    }

    public Vector2[] Normalize(Vector2[] points, int n, Step step = Step.TRANSLATED)
    {
        Vector2[] resampledPoints = PreprocessUtils.Resample(points, n);
        Vector2[] rotatedPoints = PreprocessUtils.RotateToZero(resampledPoints);
        Vector2[] scaledPoints = PreprocessUtils.ScaleToSquare(rotatedPoints, _size);
        Vector2[] translatedToOrigin = PreprocessUtils.TranslateToOrigin(scaledPoints);

        //Debug purpose only
        switch (step)
        {
            case Step.RAW:
                return points;
            case Step.RESAMPLED:
                return resampledPoints;
            case Step.ROTATED:
                return rotatedPoints;
            case Step.SCALED:
                return scaledPoints;
        }

        return translatedToOrigin;
    }

    public (string, float) DoRecognition(Vector2[] points, int n,
    List<GestureTemplate> gestureTemplates, string label = "")
    {
        Vector2[] preparedPoints = Normalize(points, n);
        float angle = GoldenRatio;
        return Recognize(preparedPoints, gestureTemplates, 250, angle, label);
    }

    private (string, float) Recognize(
        Vector2[] points,
        List<GestureTemplate> gestureTemplates,
        float size,
        float angle,
        string label = "")
    {
        float bestDistance = float.MaxValue;
        GestureTemplate bestTemplate = new GestureTemplate();

        List<GestureTemplate> filteredTemplates;

        if (string.IsNullOrEmpty(label))
        {
            filteredTemplates = gestureTemplates;
        }
        else
        {
            filteredTemplates = new List<GestureTemplate>();

            for (int i = 0; i < gestureTemplates.Count; i++)
            {
                if (gestureTemplates[i].Name == label)
                    filteredTemplates.Add(gestureTemplates[i]);
            }
        }

        foreach (GestureTemplate gestureTemplate in filteredTemplates)
        {
            float distance = DistanceAtBestAngle(points, gestureTemplate, -Theta, Theta, DeltaTheta, angle);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTemplate = gestureTemplate;
            }
        }

        double score = 1 - (bestDistance / (0.5f * Math.Sqrt(2 * size * size)));
        return ((string, float)) (bestTemplate.Name, score);
    }

    private float DistanceAtBestAngle(Vector2[] points, GestureTemplate template, float thetaA,
        float thetaB,
        float deltaTheta, float angle)
    {
        float firstX = angle * thetaA + (1 - angle) * thetaB;
        float firstDistance = DistanceAtAngle(points, template, firstX);
        float secondX = (1 - angle) * thetaA + angle * thetaB;
        float secondDistance = DistanceAtAngle(points, template, secondX);

        while (thetaB - thetaA > deltaTheta)
        {
            if (firstDistance < secondDistance)
            {
                thetaB = secondX;
                secondX = firstX;
                secondDistance = firstDistance;
                firstX = angle * thetaA + (1 - angle) * thetaB;
                firstDistance = DistanceAtAngle(points, template, firstX);
            }
            else
            {
                thetaA = firstX;
                firstX = secondX;
                firstDistance = secondDistance;
                secondX = (1 - angle) * thetaA + angle * thetaB;
                secondDistance = DistanceAtAngle(points, template, secondX);
            }
        }

        return Mathf.Min(firstDistance, secondDistance);
    }

    private float DistanceAtAngle(Vector2[] points, GestureTemplate template, float angle)
    {
        List<Vector2> newPoints = PreprocessUtils.RotateBy(points, angle);
        return PathDistance(newPoints, template.Points);
    }

    private float PathDistance(List<Vector2> points, Vector2[] templatePoints)
    {
        float distance = 0;
        int count = Mathf.Min(points.Count, templatePoints.Length);

        for (int i = 0; i < count; i++)
        {
            distance += Vector2.Distance(points[i], templatePoints[i]);
        }

        return distance / count;
    }
}