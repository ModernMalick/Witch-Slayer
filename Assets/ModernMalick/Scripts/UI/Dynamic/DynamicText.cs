using System;
using ModernMalick.Core.MonoBehaviourExtensions;
using ModernMalick.UI.Managers;
using TMPro;
using UnityEngine;

namespace ModernMalick.UI.Dynamic
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DynamicText : MonoBehaviourExtended
    {
        [SerializeField] private string prefix;
        [SerializeField] private string suffix;
        [SerializeField] private bool animate = true;

        [Component] private TextMeshProUGUI _textMesh;

        public void UpdateText(string content)
        {
            SetText(content);
        }

        public void UpdateText(int number)
        {
            SetText(number.ToString());
        }

        public void UpdateText(float number)
        {
            SetText(number.ToString());
        }

        public void UpdateText(TimeSpan time)
        {
            var text = $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
            SetText(text);
        }

        private void SetText(string value)
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMeshProUGUI>();
            }
            
            _textMesh.text = $"{prefix}{value}{suffix}";
            if (animate)
            {
                UITweener.Instance.ValueChangeTween(gameObject);
            }
        }
    }
}