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
        //string path = Application.persistentDataPath + "/SavedTemplates.json";
        //string potion = JsonUtility.ToJson(this);
        //File.WriteAllText(path, potion);
    }

    private void Load()
    {
        string defaultPath = Application.streamingAssetsPath + "/SavedTemplates.json";

        if (File.Exists(defaultPath))
        {
            LoadFromPath(defaultPath);
        }

        //string persistentPath = Application.persistentDataPath + "/SavedTemplates.json";

        //if (File.Exists(persistentPath))
        //{
        //    LoadFromPath(persistentPath);
        //    return;
        //}
    }

    private void LoadFromPath(string path)
    {
        var data = JsonUtility.FromJson<GestureTemplates>(File.ReadAllText(path));

        RawTemplates.Clear();
        RawTemplates.AddRange(data.RawTemplates);

        ProceedTemplates.Clear();
        ProceedTemplates.AddRange(data.ProceedTemplates);
    }
}