using UnityEngine;
using UnityEngine.UI;

namespace ModernMalick.UI.Dynamic.Buttons
{
    [RequireComponent(typeof(Button))]
    public abstract class AButton : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        protected abstract void OnClick();
    }
}