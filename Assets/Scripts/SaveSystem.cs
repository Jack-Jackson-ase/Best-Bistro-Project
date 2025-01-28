using System;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public static void SaveData <T>(T data, string fileName)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(GetFilePath(fileName), json);
    }

    public static T LoadData<T>(string fileName)
    {
        string path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        Debug.LogWarning($"No save file found at {path}");
        return default;
    }
}
