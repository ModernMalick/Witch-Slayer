using System.Collections.Generic;
using UnityEngine;

namespace ModernMalick.Core.Components
{
    public class VisionCone : MonoBehaviour
    {
        [Header("Rays")]
        [SerializeField] private int rayCount = 5;
        [SerializeField] private Color rayColor = Color.red;
        
        [Header("Geometry")]
        [SerializeField] private float distance = 5f;
        [SerializeField] private float angle = 60f;
        [SerializeField] private LayerMask hitMask;

        public readonly List<RaycastHit> currentHits =  new();

        private void Update()
        {            
            currentHits.Clear();
            
            var origin = transform.position;
            var halfAngle = angle * 0.5f;

            for (var i = 0; i < rayCount; i++)
            {
                var t = rayCount == 1 ? 0.5f : i / (float)(rayCount - 1);
                var currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);

                var dir = Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward;

                Physics.Raycast(origin, dir, out var hit, distance, hitMask);
                
                if (hit.collider == null) continue;
                
                currentHits.Add(hit);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = rayColor;

            var origin = transform.position;
            var halfAngle = angle * 0.5f;

            for (var i = 0; i < rayCount; i++)
            {
                var t = rayCount == 1 ? 0.5f : i / (float)(rayCount - 1);
                var currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);

                var dir = Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward;
                Gizmos.DrawRay(origin, dir * distance);
            }
        }
    }
}