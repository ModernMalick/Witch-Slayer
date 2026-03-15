using System.Linq;
using ModernMalick.Core;
using ModernMalick.Core.Components;
using UnityEngine;

namespace ModernMalick.Enemies
{
    public class EnemyVision : MonoBehaviour
    {
        [SerializeField] private VisionCone visionCone;
        [SerializeField] private float actionRange;
        
        public Transform Target { get; private set; }
        public Vector3 LastKnownTargetPosition { get; private set; }

        private void Awake()
        {
            Target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        public bool IsTargetInView()
        {
            return visionCone.currentHits.Any(hit => hit.collider.CompareTag("Player"));
        }

        public bool IsTargetInActionRange()
        {
            var targetPosition = Target.position;
            targetPosition.y = Target.position.y;
            return Vector3.Distance(transform.position, targetPosition) <= actionRange;
        }

        private void Update()
        {
            visionCone.transform.LookAt(Target.position);

            if (!IsTargetInView()) return;
            LastKnownTargetPosition = Target.position;
            var targetPosition = LastKnownTargetPosition;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
    }
}