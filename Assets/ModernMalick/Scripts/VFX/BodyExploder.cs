using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModernMalick.VFX
{
    public class BodyExploder : MonoBehaviour
    {
        [SerializeField] private float explosionForce = 300f;
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private float upwardsModifier = 0.5f;
        [SerializeField] private float cleanupDelay = 5f;

        private List<Rigidbody> _rigidbodies;

        private void Start()
        {
            _rigidbodies = new List<Rigidbody>();
            
            var body = GetComponent<Rigidbody>();
            if (body)
            {
                _rigidbodies.Add(body);
            }
            
            _rigidbodies = GetComponentsInChildren<Rigidbody>().ToList();

            var center = transform.position;

            foreach (var rb in _rigidbodies)
            {
                rb.AddExplosionForce(
                    explosionForce,
                    center,
                    explosionRadius,
                    upwardsModifier,
                    ForceMode.Impulse
                );
            }

            Invoke(nameof(Cleanup), cleanupDelay);
        }

        private void Cleanup()
        {
            foreach (var rb in _rigidbodies)
            {
                if (!rb) continue;

                var boxCollider = rb.GetComponent<BoxCollider>();

                if (boxCollider)
                {
                    Destroy(boxCollider);
                }

                Destroy(rb);
            }
        }
    }
}