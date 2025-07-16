using UnityEditor;
using UnityEngine;

namespace Assets.SimpleSignIn.Discord.Scripts.Editor
{
    [CustomEditor(typeof(DiscordAuthSettings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (DiscordAuthSettings) target;
            var warning = settings.Validate();

            if (warning != null)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            DrawDefaultInspector();

            if (GUILayout.Button("Discord for developers / Applications"))
            {
                Application.OpenURL("https://discord.com/developers/applications");
            }

            if (GUILayout.Button("Wiki"))
            {
                Application.OpenURL("https://github.com/hippogamesunity/SimpleSignIn/wiki/Discord");
            }
        }
    }
}