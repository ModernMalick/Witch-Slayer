using System;
using UnityEngine;

namespace ModernMalick.Player.Arsenal.Guns
{
    [CreateAssetMenu(menuName = "MM/Ammo")]
    public class AmmoData : ScriptableObject
    {
        public bool infiniteAmmo;
        public int startingAmmo;
        
        private int _currentAmmo;
        public int CurrentAmmo
        {
            get =>  _currentAmmo;
            private set
            {
                _currentAmmo = value;
                OnAmmoChanged.Invoke(value);
            }
        }
        
        public event Action<int> OnAmmoChanged = delegate { };

        public void Initialize()
        {
            CurrentAmmo = startingAmmo;
        }

        public bool CanConsume()
        {
            return CurrentAmmo > 0;
        }
        
        public void Consume()
        {
            CurrentAmmo--;
        }
    }
}