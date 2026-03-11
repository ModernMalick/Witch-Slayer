using System;
using UnityEngine;

namespace ModernMalick.Player.Arsenal.Guns
{
    [CreateAssetMenu(menuName = "MM/Ammo")]
    public class AmmoData : ScriptableObject
    {
        public int startingReserveAmmo;
        public int maxReserveAmmo;
        
        private int _currentReserveAmmo;
        public int CurrentReserveAmmo
        {
            get =>  _currentReserveAmmo;
            private set
            {
                _currentReserveAmmo = value;
                OnCurrentReserveAmmoChanged.Invoke(value);
            }
        }
        
        public event Action<int> OnCurrentReserveAmmoChanged = delegate { };

        public void Initialize()
        {
            CurrentReserveAmmo = startingReserveAmmo;
        }

        public int TopUp(int amount)
        {
            var space = maxReserveAmmo - CurrentReserveAmmo;
            if (space <= 0) return amount;
            var added = Mathf.Min(space, amount);
            CurrentReserveAmmo += added;
            return amount - added;
        }
        
        public int Consume(int amount)
        {
            var taken = Mathf.Min(CurrentReserveAmmo, amount);
            CurrentReserveAmmo -= taken;
            return taken;
        }
    }
}