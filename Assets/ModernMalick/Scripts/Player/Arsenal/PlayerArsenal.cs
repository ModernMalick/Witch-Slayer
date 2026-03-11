using System;
using System.Collections.Generic;
using ModernMalick.Player.Arsenal.Guns;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ModernMalick.Player.Arsenal
{
    public class PlayerArsenal : MonoBehaviour
    {
        [SerializeField] private List<Gun> guns;
        [SerializeField] private PlayerIK weaponIk;
        [SerializeField] private RectTransform crosshairRoot;
        
        private int _selectedSlotIndex;
        
        public int SelectedSlotIndex
        {
            get => _selectedSlotIndex;
            set
            {
                GetCurrentGun().gameObject.SetActive(false);
                
                _selectedSlotIndex = value;
                OnGunChanged.Invoke(guns[_selectedSlotIndex]);
                
                GetCurrentGun().gameObject.SetActive(true);
                
                if (weaponIk)
                {
                    weaponIk.SetWeapon(GetCurrentGun());
                }
            }
        }
        
        public event Action<Gun> OnGunChanged = delegate { };
        
        private bool _shootInput;

        private void Start()
        {
            if(guns == null || guns.Count == 0)
            {
                guns = new List<Gun>();
                return;
            }
            SelectedSlotIndex = 0;
        }
        
        private void Update()
        {
            if(!_shootInput || GetCurrentGun() == null) return;
            GetCurrentGun().TryAttack();
            if(GetCurrentGun().isAutomatic) return;
            _shootInput = false;
        }
        
        public void OnShoot(InputValue value)
        {
            _shootInput = value.isPressed;
        }

        public void OnReload(InputValue value)
        {
            if(!value.isPressed) return;
            GetCurrentGun().Reload();
        }

        public void OnScroll(InputValue value)
        {
            var scroll = value.Get<float>();
            switch (scroll)
            {
                case > 0:
                    ChangeSlot(-1);
                    break;
                case < 0:
                    ChangeSlot(1);
                    break;
            }
        }

        public void OnSelect(InputValue val)
        {
            var index = Mathf.RoundToInt(val.Get<float>()) - 1;
            if (index >= 0 && index < guns.Count)
                SetSlot(index);
        }
        
        private void ChangeSlot(int direction)
        {
            SelectedSlotIndex = (SelectedSlotIndex + direction + guns.Count) % guns.Count;
        }

        private void SetSlot(int index)
        {
            SelectedSlotIndex = index;
        }

        public Gun GetCurrentGun()
        {
            return guns[SelectedSlotIndex];
        }
    }
}