using UnityEngine;

namespace Assets.SimpleSignIn.Telegram.Scripts
{
    [CreateAssetMenu(fileName = "TelegramAuthSettings", menuName = "Simple Sign-In/Auth Settings/Telegram")]
    public class TelegramAuthSettings : ScriptableObject
    {
        public string Id = "Default";

        public long BotId;
        public string BotName;
        public string CustomUriScheme;

        [Header("Options")]
        public bool ShowTelegramWidget;
        [Tooltip("`TelegramAuth.Cancel()` method should be called manually. `User cancelled` callback will not called automatically when the user returns to the app without performing auth.")]
        public bool ManualCancellation;
        [Tooltip("Use Safari API on iOS instead of a default web browser. This option is required for passing App Store review.")]
        public bool UseSafariViewController = true;

        public string Validate()
        {
            #if UNITY_EDITOR

            if (BotId == 6942996843 || BotName == "simplesignin_bot" || CustomUriScheme == "simple.oauth")
            {
                return "Test settings are in use. They are for test purposes only and may be disabled or blocked. Please create your own Telegram bot.";
            }

            const string androidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";

            if (!System.IO.File.Exists(androidManifestPath))
            {
                return $"Android manifest is missing: {androidManifestPath}";
            }

            var scheme = $"<data android:scheme=\"{CustomUriScheme}\" />";

            if (!System.IO.File.ReadAllText(androidManifestPath).Contains(scheme))
            {
                return $"Custom URI scheme (deep linking) is missing in AndroidManifest.xml: {scheme}";
            }

            #endif

            return null;
        }
    }
}