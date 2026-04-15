using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public struct GestureTemplate
{
    public string Name;
    public DollarPoint[] Points;

    public GestureTemplate(string templateName, DollarPoint[] preparePoints)
    {
        Name = templateName;
        Points = preparePoints;
    }
}

[Serializable]
public class GestureTemplates
{
    private static GestureTemplates instance;

    public static GestureTemplates Get()
    {
        if (instance == null)
        {
            instance = new GestureTemplates();
            instance.Load();
        }

        return instance;
    }


    public List<GestureTemplate> RawTemplates = new List<GestureTemplate>();
    public List<GestureTemplate> ProceedTemplates = new List<GestureTemplate>();

    public List<GestureTemplate> GetTemplates()
    {
        return ProceedTemplates;
    }

    public void RemoveAtIndex(int indexToRemove)
    {
        ProceedTemplates.RemoveAt(indexToRemove);
        RawTemplates.RemoveAt(indexToRemove);
    }

    public GestureTemplate[] GetRawTemplatesByName(string name)
    {
        return RawTemplates.Where(template => template.Name == name).ToArray();
    }

    public void Save()
    {
#if UNITY_EDITOR
        string path = Application.dataPath + "/Resources/SavedTemplates.json";
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, json);
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void Load()
    {
        TextAsset file = Resources.Load<TextAsset>("SavedTemplates");

        if (file != null)
        {
            LoadFromJson(file.text);
        }
        else
        {
            Debug.LogError("SavedTemplates not found in Resources");
        }
    }

    private void LoadFromJson(string json)
    {
        var data = JsonUtility.FromJson<GestureTemplates>(json);

        RawTemplates.Clear();
        RawTemplates.AddRange(data.RawTemplates);

        ProceedTemplates.Clear();
        ProceedTemplates.AddRange(data.ProceedTemplates);
    }
}