using System.IO;
using UnityEngine;

public class SaveService
{
    private const string SETTINGS_FILE = "settings.json";

    public void SaveSettings(SettingsStateSO settings)
    {
        string json = JsonUtility.ToJson(settings);
        string path = Path.Combine(Application.persistentDataPath, SETTINGS_FILE);
        File.WriteAllText(path, json);
        Debug.Log($"Settings saved to: {path}");
    }

    public void LoadSettings(SettingsStateSO settings)
    {
        string path = Path.Combine(Application.persistentDataPath, SETTINGS_FILE);
        if (!File.Exists(path))
        {
            Debug.LogWarning("No settings file found. Using default values.");
            return;
        }

        string json = File.ReadAllText(path);
        JsonUtility.FromJsonOverwrite(json, settings);
        Debug.Log("Settings loaded.");
    }
}
