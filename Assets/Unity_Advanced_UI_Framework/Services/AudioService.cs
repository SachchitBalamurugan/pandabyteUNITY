using UnityEngine;

public class AudioService
{
    private AudioSource _musicSource;
    private SettingsStateSO _settings;

    public void Init(AudioSource musicSource, SettingsStateSO settings)
    {
        _musicSource = musicSource;
        _settings = settings;

        ApplySettings();
    }

    public void ToggleMusic(bool isOn)
    {
        _settings.musicOn = isOn;
        if (_musicSource != null)
            _musicSource.mute = !isOn;
    }

    public void SetVolume(float volume)
    {
        _settings.masterVolume = volume;
        if (_musicSource != null)
            _musicSource.volume = volume;
    }

    private void ApplySettings()
    {
        if (_musicSource == null) return;

        _musicSource.volume = _settings.masterVolume;
        _musicSource.mute = !_settings.musicOn;
    }
}
