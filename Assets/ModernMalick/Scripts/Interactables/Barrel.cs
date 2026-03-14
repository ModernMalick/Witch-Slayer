using System.Collections.Generic;
using ModernMalick.Core.MonoBehaviourExtensions;
using Unity.Cinemachine;
using UnityEngine;

namespace ModernMalick.Interactables
{
    public class Barrel : MonoBehaviourExtended
    {
        [Header("Explosion")] 
        [SerializeField] private int damage;
        [SerializeField] private float radius;
        [SerializeField] private LayerMask hitMask;
        [SerializeField] private LayerMask obstructionMask;

        [Header("VFX")] 
        [SerializeField] private GameObject barrel;
        [SerializeField] private GameObject vfx;
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private float destroyDelay;

        [Component] private Health.Health _health;
        
        private static readonly Collider[] COLLIDER_BUFFER = new Collider[64];

        private void OnEnable()
        {
            _health.OnHealthDecreased += _ => Explode();
        }

        public void Explode()
        {
            var hits = Physics.OverlapSphereNonAlloc(
                transform.position,
                radius,
                COLLIDER_BUFFER,
                hitMask
            );

            var origin = transform.position;
            
            for (var i = 0; i < hits; i++)
            {
                var col = COLLIDER_BUFFER[i];
                if (col == null || col.gameObject == gameObject) continue;

                var targetPoint = col.bounds.center;
                var direction = targetPoint - origin;
                var distance = direction.magnitude;

                if (Physics.Raycast(origin, direction.normalized, out var hit, distance, obstructionMask))
                {
                    if (hit.collider != col)
                        continue;
                }

                Health.Health.TryModifyHealth(col.gameObject, -damage);
            }
            
            barrel.SetActive(false);
            
            if(vfx)
            {
                vfx.SetActive(true);
            }
            
            if(impulseSource)
            {
                impulseSource.GenerateImpulse();
            }
            
            Destroy(gameObject, destroyDelay);
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}