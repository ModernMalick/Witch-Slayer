using ModernMalick.Core.MonoBehaviourExtensions;
using ModernMalick.UI.Dynamic;
using UnityEngine;

namespace ModernMalick.Health
{
    [RequireComponent(typeof(DynamicText))]
    public class HealthText : MonoBehaviourExtended
    {
        [SerializeField] private Health health;
        
        [Component] private DynamicText _text;

        private void OnEnable()
        {
            health.OnHealthChanged += _text.UpdateText;
        }

        private void OnDisable()
        {
            health.OnHealthChanged -= _text.UpdateText;
        }
    }
}