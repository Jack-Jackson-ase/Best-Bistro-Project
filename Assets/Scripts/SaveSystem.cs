using System;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static (float volume, float sensitivityRaw) GetOptionsStats() // tuple return type
    {
        if (PlayerPrefs.HasKey("Volume") != true)
        {
            PlayerPrefs.SetFloat("Volume", 0.5f);
            Debug.Log("Player Prefs Volume was zero, now 0.5f");
        }
        if (PlayerPrefs.HasKey("Mouse Sensitivity Raw") != true)
        {
            PlayerPrefs.SetFloat("Mouse Sensitivity Raw", 0.5f);
            Debug.Log("Player Prefs Mouse Sensitivity Raw was zero, now 0.5f");
        }

        float volume = PlayerPrefs.GetFloat("Volume");
        float sensitivityRaw = PlayerPrefs.GetFloat("Mouse Sensitivity Raw");

        return (volume, sensitivityRaw); // tuple literal
    }

    public static string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }
    public static bool SearchIfFileExists(string fileName)
    {
        return File.Exists(GetFilePath(fileName));
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
