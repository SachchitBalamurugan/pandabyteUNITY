using UnityEditor;
using UnityEngine;

namespace Assets.SimpleSignIn.Telegram.Scripts.Editor
{
    [CustomEditor(typeof(TelegramAuthSettings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (TelegramAuthSettings)target;
            var warning = settings.Validate();

            if (warning != null)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            DrawDefaultInspector();

            if (GUILayout.Button("Wiki"))
            {
                Application.OpenURL("https://github.com/hippogamesunity/SimpleSignIn/wiki/Telegram");
            }
        }
    }
}