using UnityEngine;

namespace ModernMalick.Common.Settings
{
    public class Quality : MonoBehaviour
    {
        [SerializeField] private int fpsTarget;
        [SerializeField] private bool vSync;

        private void Awake()
        {
            Application.targetFrameRate = fpsTarget;
            QualitySettings.vSyncCount = vSync ? 1 : 0;
        }
    }
}