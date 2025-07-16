using System.Collections.Generic;
using UnityEngine;

namespace Assets.SimpleSignIn.Microsoft.Scripts
{
    [CreateAssetMenu(fileName = "MicrosoftAuthSettings", menuName = "Simple Sign-In/Auth Settings/Microsoft")]
    public class MicrosoftAuthSettings : ScriptableObject
    {
        public string Id = "Default";
        public string ClientId;
        public string CustomUriScheme;

        [Header("Options")]
        public List<string> AccessScopes = new() { "openid", "email", "profile" };
        [Tooltip("`MicrosoftAuth.Cancel()` method should be called manually. `User cancelled` callback will not called automatically when the user returns to the app without performing auth.")]
        public bool ManualCancellation;
        [Tooltip("Use Safari API on iOS instead of a default web browser. This option is required for passing App Store review.")]
        public bool UseSafariViewController = true;

        public string Validate()
        {
            #if UNITY_EDITOR

            if (ClientId == "40e5b292-98f2-479d-8b7c-82be546dddc4" || CustomUriScheme == "simple.oauth")
            {
                return "Test settings are in use. They are for test purposes only and may be disabled or blocked. Please register your own app with custom URI scheme.";
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