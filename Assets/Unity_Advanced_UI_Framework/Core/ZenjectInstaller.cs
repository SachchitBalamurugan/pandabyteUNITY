using PandaBytes;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ZenjectInstaller : MonoInstaller
{
    [Header("Managers & Services")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private NotificationSystem notificationSystem;
    [SerializeField] private UIInputBlocker inputBlocker;
   

    [Header("Configs & State")]
    [SerializeField] private SettingsStateSO settings;
    [SerializeField] private List<LocalizationConfigSO> languageConfigs;
    [SerializeField] private List<ThemeDataSO> themePresets;

    public override void InstallBindings()
    {
       

        // ✅ Core Managers
        if (uiManager != null)
            Container.Bind<UIManager>().FromInstance(uiManager).AsSingle();
        else
            Debug.LogError("❌ UIManager not assigned.");

        if (poolManager != null)
            Container.Bind<PoolManager>().FromInstance(poolManager).AsSingle();
        else
            Debug.LogError("❌ PoolManager not assigned.");

        Container.Bind<UIStateMachine>().AsSingle();
        Container.Bind<UIFactory>().AsSingle();
        // ✅ Game State & Settings
        if (settings != null)
            Container.Bind<SettingsStateSO>().FromScriptableObject(settings).AsSingle();
        else
            Debug.LogWarning("⚠️ SettingsStateSO not assigned.");

        // ✅ Services
        Container.Bind<SaveService>().AsSingle();
        Container.Bind<AudioService>().AsSingle();

        if (languageConfigs != null && languageConfigs.Count > 0)
            Container.Bind<LocalizationService>().FromInstance(new LocalizationService(languageConfigs)).AsSingle();
        else
            Debug.LogWarning("⚠️ Localization configs not assigned.");

        if (themePresets != null && themePresets.Count > 0)
            Container.Bind<ThemeService>().FromInstance(new ThemeService(themePresets, "Light")).AsSingle();
        else
            Debug.LogWarning("⚠️ Theme presets not assigned.");

        // ✅ UI Utility Components
        if (notificationSystem != null)
            Container.Bind<NotificationSystem>().FromInstance(notificationSystem).AsSingle();
        else
            Debug.LogWarning("⚠️ NotificationSystem not assigned.");

        if (sceneLoader != null)
            Container.Bind<SceneLoader>().FromInstance(sceneLoader).AsSingle();
        else
            Debug.LogWarning("⚠️ SceneLoader not assigned.");

        if (inputBlocker != null)
            Container.Bind<UIInputBlocker>().FromInstance(inputBlocker).AsSingle();
        else
            Debug.LogWarning("⚠️ UIInputBlocker not assigned.");
    }
}
