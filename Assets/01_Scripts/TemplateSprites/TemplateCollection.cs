using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TemplateCollection", menuName = "Scriptable Objects/Template Collection")]
public class TemplateCollection : ScriptableObject
{
    public string collectionName;
    public List<Template> templates;
}

public static class TemplateCollectionExtensions
{
    private const string TemplateCollectionPath = "TemplateCollections/";
    private const string TemplateCollectionName = "LostMagic";

    private static TemplateCollection _templateCollection;
    private static Dictionary<string, Template> templateMap;
    private static List<Template> scrambledTemplates;

    private static bool _isInitialized = false;

    private static void Initialize()
    {
        if (_isInitialized)
            return;

        _templateCollection = Resources.Load<TemplateCollection>(TemplateCollectionPath + TemplateCollectionName);

        if (_templateCollection == null)
        {
            Debug.LogError("Failed to load TemplateCollection");
            return;
        }

        if (_templateCollection.templates == null || _templateCollection.templates.Count == 0)
        {
            Debug.LogError("TemplateCollection is empty");
            return;
        }

        templateMap = new Dictionary<string, Template>();

        foreach (var t in _templateCollection.templates)
        {
            templateMap[t.label] = t;
        }

        scrambledTemplates = new List<Template>(_templateCollection.templates);

        Shuffle();

        _isInitialized = true;
    }

    private static void Shuffle()
    {
        for (int i = scrambledTemplates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = scrambledTemplates[i];
            scrambledTemplates[i] = scrambledTemplates[j];
            scrambledTemplates[j] = temp;
        }
    }

    public static Template GetTemplate(string label)
    {
        Initialize();

        if (templateMap.TryGetValue(label, out var template))
        {
            return template;
        }

        Debug.LogWarning("Template not found: " + label);
        return null;
    }

    public static Sprite GetSprite(string label)
    {
        var template = GetTemplate(label);
        return template != null ? template.sprite : null;
    }

    public static Template GetRandomTemplate()
    {
        Initialize();

        if (scrambledTemplates.Count == 0)
        {
            scrambledTemplates = new List<Template>(_templateCollection.templates);
            Shuffle();
        }

        var template = scrambledTemplates[0];
        scrambledTemplates.RemoveAt(0);

        return template;
    }

    public static int GetTemplateCount()
    {
        Initialize();
        return _templateCollection != null ? _templateCollection.templates.Count : 0;
    }

    public static int GetRemainingRandomTemplates()
    {
        Initialize();
        return scrambledTemplates != null ? scrambledTemplates.Count : 0;
    }

    public static void Reset()
    {
        Initialize();
        scrambledTemplates = new List<Template>(_templateCollection.templates);
        Shuffle();
    }
}