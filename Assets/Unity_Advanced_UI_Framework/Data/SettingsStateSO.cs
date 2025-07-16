using UnityEngine;

[CreateAssetMenu(menuName = "UI/Settings State")]
public class SettingsStateSO : ScriptableObject
{
    public bool musicOn = true;
    public float masterVolume = 1.0f;
    public string language = "en";
    public string languageCode = "en";
    public string themeName = "Light";
}
