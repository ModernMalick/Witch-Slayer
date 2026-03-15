using System.Collections.Generic;
using UnityEngine;

namespace ModernMalick.VFX.Surface
{
    public class ParticleCollider : MonoBehaviour
    {
        [SerializeField] private GameObject impactObject;
        
        private ParticleSystem _ps;
        private List<ParticleCollisionEvent> _events;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
            _events = new List<ParticleCollisionEvent>();
        }

        private void OnParticleCollision(GameObject other)
        {
            int count = _ps.GetCollisionEvents(other, _events);

            for (int i = 0; i < count; i++)
            {
                var e = _events[i];

                var rotation = Quaternion.LookRotation(-e.normal);
                Instantiate(impactObject, e.intersection, rotation);
            }
        }
    }
}