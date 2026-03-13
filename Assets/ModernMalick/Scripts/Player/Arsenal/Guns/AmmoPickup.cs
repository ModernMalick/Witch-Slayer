using ModernMalick.Audio;
using UnityEngine;

namespace ModernMalick.Player.Arsenal.Guns
{
    public class AmmoPickup : Pickup
    {
        [SerializeField] private int ammoBonus;
        [SerializeField] private AmmoData ammoData;
        [SerializeField] private AudioClip pickupClip;
        
        protected override bool TryPickup(GameObject other)
        {
            ammoData.AddAmmo(ammoBonus);
            if (pickupClip)
            {
                AudioManager.Instance.AudioSource.PlayOneShot(pickupClip);
            }
            return true;
        }
    }
}