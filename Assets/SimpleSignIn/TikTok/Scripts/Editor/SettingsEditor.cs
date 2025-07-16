using UnityEditor;
using UnityEngine;

namespace Assets.SimpleSignIn.TikTok.Scripts.Editor
{
    [CustomEditor(typeof(TikTokAuthSettings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (TikTokAuthSettings) target;
            var warning = settings.Validate();

            if (warning != null)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            DrawDefaultInspector();

            if (GUILayout.Button("TikTok for developers"))
            {
                Application.OpenURL("https://developers.tiktok.com/apps/");
            }

            if (GUILayout.Button("Wiki"))
            {
                Application.OpenURL("https://github.com/hippogamesunity/SimpleSignIn/wiki/TikTok");
            }
        }
    }
}