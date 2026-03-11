using System;
using UnityEngine;

namespace ModernMalick.Health
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int startHealth;
        [SerializeField] private int maxHealth;
        
        public event Action<int> OnHealthChanged = delegate { };
        public event Action<int> OnHealthModified = delegate { };
        public event Action<float> OnHealthPercentageChanged = delegate { };
        
        public event Action<int> OnHealthIncreased = delegate { };
        public event Action<int> OnHealthDecreased = delegate { };

        public event Action OnHealthDepleted = delegate { };
        
        private int _currentHealth;
        private int CurrentHealth
        {
            get => _currentHealth;
            set
            {
                var clamped = Mathf.Clamp(value, 0, maxHealth);
                var delta = clamped - _currentHealth;

                _currentHealth = clamped;

                if (!_initialized)
                {
                    _initialized = true;
                    return;
                }
                
                OnHealthChanged.Invoke(_currentHealth);
                OnHealthPercentageChanged.Invoke(_currentHealth / (float)maxHealth);

                switch (delta)
                {
                    case > 0:
                        OnHealthIncreased.Invoke(delta);
                        break;
                    case < 0:
                        OnHealthDecreased.Invoke(-delta);
                        break;
                }

                if (_currentHealth != 0) return;
                OnHealthDepleted.Invoke();
            }
        }
        
        private bool _initialized;

        private void Awake()
        {
            CurrentHealth = startHealth;
        }

        public void ModifyHealth(int delta)
        {
            CurrentHealth += delta;
            OnHealthModified.Invoke(delta);
        }

        public static bool TryModifyHealth(GameObject other, int delta)
        {
            var health = other.GetComponent<Health>();
            if(!health) return false;
            health.ModifyHealth(delta);
            return true;
        }

        public bool IsHealthFull()
        {
            return CurrentHealth == maxHealth;
        }
    }
}