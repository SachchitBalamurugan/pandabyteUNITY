using UnityEngine;

namespace Assets.SimpleSignIn.VK.Scripts
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Simple Sign-In/Auth Settings/VK")]
    public class VKAuthSettings : ScriptableObject
    {
        [SerializeField] private string _appId;
        [SerializeField] private string _secureKey;
        public string CustomUriScheme;

        public string ClientId => _appId;
        public string ClientSecret => _secureKey;

        private static VKAuthSettings _instance;

        public static VKAuthSettings Instance => _instance ??= Resources.Load<VKAuthSettings>("Settings");

        public string Validate()
        {
            #if UNITY_EDITOR

            if (ClientId == "51732203" || ClientSecret == "j3lvv5oL98V6qjaKGpPp" || CustomUriScheme == "simple.oauth")
            {
                return "Test settings are in use. They are for test purposes only and may be disabled or blocked. Please create your own app with custom URI scheme.";
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