using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModernMalick.Player.Movement
{
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerGround : MonoBehaviour
    {
        private List<Collider> _groundCollisions;
        
        public event Action OnLanded = delegate { };
        
        private void Awake()
        {
            _groundCollisions = new List<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            OnTrigger(other);
        }

        private void OnTriggerStay(Collider other)
        {
            OnTrigger(other);
        }

        private void OnTrigger(Collider other)
        {
            if (!IsGrounded())
            {
                OnLanded.Invoke();
            }
            
            if(_groundCollisions.Contains(other)) return;
            
            _groundCollisions.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            _groundCollisions.Remove(other);
        }

        public bool IsGrounded()
        {
            _groundCollisions.RemoveAll(colliderObjects => !colliderObjects || !colliderObjects.gameObject.activeInHierarchy);
            return _groundCollisions.Count > 0;
        }
    }
}