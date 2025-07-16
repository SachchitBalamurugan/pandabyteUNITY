using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.SimpleSignIn.TikTok.Scripts
{
    [CreateAssetMenu(fileName = "TikTokAuthSettings", menuName = "Simple Sign-In/Auth Settings/TikTok")]
    public class TikTokAuthSettings : ScriptableObject
    {
        public string Id = "Default";
        public string ClientKey;
        public string ClientSecret;
        public string CustomUriScheme;
        public string AndroidSha256Fingerprint; // keytool -keystore path-to-debug-or-production-keystore -list -v

        [Header("Options")]
        public List<string> AccessScopes = new() { "user.info.basic" };
        public List<string> UserFields = new() { "open_id", "union_id", "avatar_url", "display_name" };
        [Tooltip("`TikTokAuth.Cancel()` method should be called manually. `User cancelled` callback will not called automatically when the user returns to the app without performing auth.")]
        public bool ManualCancellation;
        [Tooltip("Use Safari API on iOS instead of a default web browser. This option is required for passing App Store review.")]
        public bool UseSafariViewController = true;

        [Header("Flow setup")]
        public OAuthFlow FlowForAndroid;
        [FormerlySerializedAs("FlowForiOS")] public OAuthFlow FlowForIOS;
        public OAuthFlow FlowForWindows;
        public OAuthFlow FlowForUWP;
        public OAuthFlow FlowForOSX;
        public string PackageName;

        public string Validate()
        {
            #if UNITY_EDITOR

            if (ClientKey == "sbawmk4asw2oj1r60m" || ClientSecret == "ywhVDttA1Ih7V0FkASyDrjrHMIZyXsOw" || CustomUriScheme == "simple.oauth")
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

            if (AndroidSha256Fingerprint.Length > 0 && AndroidSha256Fingerprint.Length != 95)
            {
                return $"{nameof(AndroidSha256Fingerprint)} should be 95 characters long";
            }

            #endif

            return null;
        }
    }

    public enum OAuthFlow
    {
        Loopback,
        AuthorizationMiddleware,
        Android,
        iOS
    }
}