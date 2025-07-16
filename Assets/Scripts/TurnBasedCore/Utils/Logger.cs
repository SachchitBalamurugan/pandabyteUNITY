using UnityEngine;
using TurnBasedCore.Core.TurnSystem;

namespace TurnBasedCore.Core.Utils
{
    public static class Logger
    {
        private static TurnSettingsSO settings;

        public static void Initialize(TurnSettingsSO settingsSO)
        {
            settings = settingsSO;
        }

        public static void Log(string message)
        {
            if (settings != null && settings.EnableDebugLogs)
                Debug.Log("[LOG] " + message);
        }

        public static void Warn(string message)
        {
            if (settings != null && settings.EnableDebugLogs)
                Debug.LogWarning("[WARN] " + message);
        }

        public static void Error(string message)
        {
            if (settings != null && settings.EnableDebugLogs)
                Debug.LogError("[ERROR] " + message);
        }
    }
}
