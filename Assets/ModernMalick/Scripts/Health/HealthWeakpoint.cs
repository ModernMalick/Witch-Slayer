using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;

namespace ModernMalick.Health
{
    [RequireComponent(typeof(Health))]
    public class HealthWeakpoint : MonoBehaviourExtended
    {
        [SerializeField] private Health targetHealth;
        [SerializeField] private int multiplier = 2;

        [Component] private Health _health;

        private void OnEnable()
        {
            _health.OnHealthModified += MultiplyEffect;
        }

        private void OnDisable()
        {
            _health.OnHealthModified -= MultiplyEffect;
        }

        private void MultiplyEffect(int delta)
        {
            targetHealth.ModifyHealth(multiplier * delta);
        }
    }
}