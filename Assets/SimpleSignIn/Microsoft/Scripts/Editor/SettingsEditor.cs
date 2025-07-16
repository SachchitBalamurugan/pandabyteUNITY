using UnityEditor;
using UnityEngine;

namespace Assets.SimpleSignIn.Microsoft.Scripts.Editor
{
    [CustomEditor(typeof(MicrosoftAuthSettings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (MicrosoftAuthSettings) target;
            var warning = settings.Validate();

            if (warning != null)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            DrawDefaultInspector();

            if (GUILayout.Button("Microsoft Entra admin center"))
            {
                Application.OpenURL("https://entra.microsoft.com/");
            }

            if (GUILayout.Button("Wiki"))
            {
                Application.OpenURL("https://github.com/hippogamesunity/SimpleSignIn/wiki/Microsoft");
            }
        }
    }
}