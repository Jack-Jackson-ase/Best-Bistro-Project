using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string gameState;
    public int roundNumber;
    public int health;
    public int hunger;

    public SaveData(string gameState, int roundNumber, int health, int hunger)
    {
        this.gameState = gameState;
        this.roundNumber = roundNumber;
        this.health = health;
        this.hunger = hunger;
    }
}

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

    public static void SaveGameState(string fileName, string gameState, int roundNumber, int hunger, int health)
    {
        SaveData saveData = new SaveData(gameState, roundNumber, health, hunger);
        SaveDataToFile(fileName, saveData);
    }
    public static void DeleteSaveGame(string fileName)
    {
        string path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Save file {fileName} was deleted.");
        }
        else
        {
            Debug.LogWarning($"No save file found at {path} to delete.");
        }
    }

    public static SaveData LoadSaveGameState(string fileName)
    {
        return LoadDataFromFile<SaveData>(fileName);
    }

    public static string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }
    public static bool SearchIfFileExists(string fileName)
    {
        return File.Exists(GetFilePath(fileName));
    }

    public static void SaveDataToFile<T>(string fileName, T data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(GetFilePath(fileName), json);
    }

    public static T LoadDataFromFile<T>(string fileName)
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