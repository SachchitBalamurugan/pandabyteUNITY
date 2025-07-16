using System.Collections.Generic;
using UnityEngine;

namespace Assets.SimpleSignIn.Discord.Scripts
{
    [CreateAssetMenu(fileName = "DiscordAuthSettings", menuName = "Simple Sign-In/Auth Settings/Discord")]
    public class DiscordAuthSettings : ScriptableObject
    {
        public string Id = "Default";

        public string ClientId;
        public string ClientSecret;
        public string CustomUriScheme;

        [Header("Options")]
        public List<string> AccessScopes = new() { "openid", "email", "profile" };
        [Tooltip("Controls how the authorization flow handles existing authorizations. Use `consent` to reapprove user authorization each time, or `none` to skip the authorization screen.")]
        public string Prompt = "consent";
        [Tooltip("Specifies the installation context for the authorization. 0 (GUILD_INSTALL) or 1 (USER_INSTALL).")]
        public int IntegrationType = 0;
        [Tooltip("`DiscordAuth.Cancel()` method should be called manually. `User cancelled` callback will not called automatically when the user returns to the app without performing auth.")]
        public bool ManualCancellation;
        [Tooltip("Use Safari API on iOS instead of a default web browser. This option is required for passing App Store review.")]
        public bool UseSafariViewController = true;

        public string Validate()
        {
            #if UNITY_EDITOR

            if (ClientId == "1294559229784358996" || ClientSecret == "nA81YCPiA36UCkw5W5io5sOwDTXEYb71" || CustomUriScheme == "simple.oauth")
            {
                return "Test settings are in use. They are for test purposes only and may be disabled or blocked. Please register your own credentials and custom URI scheme.";
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