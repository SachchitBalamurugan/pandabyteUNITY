using UnityEditor;
using UnityEngine;

namespace Assets.SimpleSignIn.VK.Scripts.Editor
{
    [CustomEditor(typeof(VKAuthSettings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (VKAuthSettings) target;
            var warning = settings.Validate();

            if (warning != null)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            DrawDefaultInspector();

            if (GUILayout.Button("VK Developers"))
            {
                Application.OpenURL("https://dev.vk.com/ru");
            }

            if (GUILayout.Button("Wiki"))
            {
                Application.OpenURL("https://github.com/hippogamesunity/SimpleSignIn/wiki/VK");
            }
        }
    }
}