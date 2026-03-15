using ModernMalick.Core.Components;
using ModernMalick.Player;
using UnityEngine;

namespace ModernMalick.Health
{
    public class HealthPickup : Pickup
    {
        [SerializeField] private int healAmount;
        
        protected override bool TryPickup(GameObject other)
        {
            var health = other.GetComponent<Health>();
            if (health == null || health.IsHealthFull()) return false;
            health.ModifyHealth(healAmount);
            return true;
        }
    }
}