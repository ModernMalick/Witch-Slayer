using ModernMalick.Core.MonoBehaviourExtensions;
using ModernMalick.UI.Dynamic;
using UnityEngine;

namespace ModernMalick.Health
{
    [RequireComponent(typeof(DynamicProgress))]
    public class HealthProgress : MonoBehaviourExtended
    {
        [SerializeField] private Health health;
        
        [Component] private DynamicProgress _progress;

        private void OnEnable()
        {
            health.OnHealthPercentageChanged += _progress.UpdateProgress;
        }

        private void OnDisable()
        {
            health.OnHealthPercentageChanged -= _progress.UpdateProgress;
        }
    }
}