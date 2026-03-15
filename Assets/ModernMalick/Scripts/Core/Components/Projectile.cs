using System;
using System.Collections.Generic;
using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;

namespace ModernMalick.Core.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviourExtended
    {
        [SerializeField] private float speed;
        [SerializeField] private float range;
        
        [Header("Collision")]
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private List<GameObject> impactObjects;
        
        [Component] private Rigidbody _rigidbody;
        
        private Vector3 _startPosition;
        private bool _stopped;
        private Vector3 _lastPosition;
        private List<Collider> _collided;
        
        public event Action<RaycastHit> OnImpact = delegate { };
        
        private void Start()
        {
            _lastPosition = transform.position;
            _collided = new List<Collider>();
        }
        
        private void Update()
        {
            CheckRange();
            CheckForHits();
            _lastPosition = transform.position;
        }
        
        public void Fire()
        {
            _startPosition = transform.position;
            _rigidbody.linearVelocity = transform.forward * speed;
        }

        private void CheckRange()
        {
            if (_stopped || Vector3.Distance(_startPosition, transform.position) < range) return;
            Destroy(gameObject);
        }
        
        private void CheckForHits()
        {
            if (_stopped) return;
            
            var displacement = transform.position - _lastPosition;
            var distance = displacement.magnitude;

            if (distance <= 0f) return;

            var hits = Physics.RaycastAll(_lastPosition, displacement.normalized, distance * 2, layerMask);
            if (hits.Length == 0) return;

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (_collided.Contains(hit.collider)) continue;
                _collided.Add(hit.collider);
                OnImpact.Invoke(hit);
                
                foreach (var impactObject in impactObjects)
                {
                    Instantiate(impactObject, hit.point, Quaternion.LookRotation(-hit.normal));
                }
                
                return;
            }
            
            Destroy(gameObject);
        }
    }
}