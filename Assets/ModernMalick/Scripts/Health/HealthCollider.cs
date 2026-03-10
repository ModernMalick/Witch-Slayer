using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernMalick.Health
{
    public class HealthCollider : MonoBehaviour
    {
        [SerializeField] private int healthChangeAmount;

        [Header("Repetition")] 
        [SerializeField] private bool isRepeating;
        [SerializeField] private float repetitionRate;
        
        private List<Health> _currentCollisions;

        public event Action<Health> OnHealthModified = delegate { };

        private void Awake()
        {
            _currentCollisions = new List<Health>();
        }

        private void Start()
        {
            InvokeRepeating(nameof(ModifyHealthRepeating), repetitionRate, repetitionRate);
        }

        private void OnTriggerEnter(Collider other)
        {
            var health = other.gameObject.GetComponent<Health>();
            if(!health) return;
            if(_currentCollisions.Contains(health)) return;
            _currentCollisions.Add(health);
            
            if(isRepeating) return;
            health.ModifyHealth(healthChangeAmount);
            OnHealthModified.Invoke(health);
        }

        private void OnTriggerExit(Collider other)
        {
            var health = other.gameObject.GetComponent<Health>();
            if(!health) return;
            if(!_currentCollisions.Contains(health)) return;
            _currentCollisions.Remove(health);
        }

        private void ModifyHealthRepeating()
        {
            _currentCollisions.ForEach(health =>
            {
                health.ModifyHealth(healthChangeAmount);
                OnHealthModified.Invoke(health);
            });
        }
    }
}