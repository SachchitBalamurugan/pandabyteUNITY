using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Localization/Config")]
public class LocalizationConfigSO : ScriptableObject
{
    public string languageCode = "en"; // e.g., en, ur, ar

    [System.Serializable]
    public class LocalizedEntry
    {
        public string key;
        public string value;
    }

    public List<LocalizedEntry> entries;

    public Dictionary<string, string> GetDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in entries)
        {
            dict[entry.key] = entry.value;
        }
        return dict;
    }
}
