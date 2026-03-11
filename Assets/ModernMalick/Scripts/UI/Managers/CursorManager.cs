using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;

namespace ModernMalick.UI.Managers
{
    public class CursorManager : MonoBehaviourSingleton<CursorManager>
    {
        [SerializeField] private bool showOnStart;

        private void Start()
        {
            SetCursor(showOnStart);
        }

        public void SetCursor(bool active)
        {
            Cursor.visible = active;
            Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}