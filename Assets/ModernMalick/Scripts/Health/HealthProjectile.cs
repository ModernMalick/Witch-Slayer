using ModernMalick.Core.Components;
using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;

namespace ModernMalick.Health
{
    [RequireComponent(typeof(Projectile))]
    public class HealthProjectile : MonoBehaviourExtended
    {
        [SerializeField] private int healthChange;

        [Component] private Projectile _projectile;
        
        private void OnEnable()
        {
            _projectile.OnImpact += OnImpact;
        }

        private void OnDisable()
        {
            _projectile.OnImpact += OnImpact;
        }

        private void OnImpact(RaycastHit hit)
        {
            Health.TryModifyHealth(hit.collider.gameObject, healthChange);
        }
    }
}