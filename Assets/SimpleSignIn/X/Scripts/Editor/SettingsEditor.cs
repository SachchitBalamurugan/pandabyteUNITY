using UnityEditor;
using UnityEngine;

namespace Assets.SimpleSignIn.X.Scripts.Editor
{
    [CustomEditor(typeof(XAuthSettings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (XAuthSettings) target;
            var warning = settings.Validate();

            if (warning != null)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            DrawDefaultInspector();

            if (GUILayout.Button("X Developer Portal"))
            {
                Application.OpenURL("https://developer.twitter.com/en/portal/dashboard");
            }

            if (GUILayout.Button("Wiki"))
            {
                Application.OpenURL("https://github.com/hippogamesunity/SimpleSignIn/wiki/X");
            }
        }
    }
}