using System.Collections.Generic;
using UnityEngine;

namespace Assets.SimpleSignIn.X.Scripts
{
    [CreateAssetMenu(fileName = "XAuthSettings", menuName = "Simple Sign-In/Auth Settings/X")]
    public class XAuthSettings : ScriptableObject
    {
        public string Id = "Default";

        public string ClientId;
        public string ClientSecret;
        public string CustomUriScheme;

        [Header("Options")]
        public List<string> AccessScopes = new() { "tweet.read", "users.read" };
        [Tooltip("`XAuth.Cancel()` method should be called manually. `User cancelled` callback will not called automatically when the user returns to the app without performing auth.")]
        public bool ManualCancellation;
        [Tooltip("Use Safari API on iOS instead of a default web browser. This option is required for passing App Store review.")]
        public bool UseSafariViewController = true;

        public string Validate()
        {
            #if UNITY_EDITOR

            if (ClientId == "VzZjeDZiZmx3N29JWVNRSXl0SHE6MTpjaQ" || ClientSecret == "7GI1D98rL6EpIczCQrNqvuZsdHHw4qs1MaQDv37NcWS5VoQ0mY" || CustomUriScheme == "simple.oauth")
            {
                return "Test settings are in use. They are for test purposes only and may be disabled or blocked. Please set your own settings obtained from X Developer Portal.";
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