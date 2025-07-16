using UnityEngine;

namespace TurnBasedCore.Core.TurnSystem
{
    [CreateAssetMenu(fileName = "TurnSettings", menuName = "TurnBasedCore/Turn Settings")]
    public class TurnSettingsSO : ScriptableObject
    {
        [Header("Turn Timer Settings")]
        public float TurnDuration = 15f;

        [Header("Auto Action Rules")]
        public bool AllowAutoAction = true;
        public string DefaultAutoActionCommand = "Fold"; // For ActionInvoker to resolve

        [Header("Logging")]
        public bool EnableDebugLogs = true;
    }
}
