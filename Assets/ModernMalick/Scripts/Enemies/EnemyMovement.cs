using ModernMalick.Core.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace ModernMalick.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyVision))]
    [RequireComponent(typeof(Animator))]
    public class EnemyMovement : State
    {
        [SerializeField] private bool chaseEnabled;

        [Header("Strafe")]
        [SerializeField] private bool strafeEnabled;
        [SerializeField] private float strafeDistance = 3f;
        
        [Component] private NavMeshAgent _agent;
        [Component] private EnemyVision _enemyVision;
        [Component] private Animator _animator;
        
        private static readonly int VELOCITY = Animator.StringToHash("Velocity");

        private Vector3 _strafeTarget;
        private bool _isStrafing;

        public override void OnEnter()
        {
            base.OnEnter();
            _agent.isStopped = false;
        }

        public override void OnExit()
        {
            base.OnExit();
            _agent.isStopped = true;
            _agent.ResetPath();
            _isStrafing = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            _animator.SetFloat(VELOCITY, Mathf.Abs(_agent.velocity.magnitude / _agent.speed));

            if (!chaseEnabled && !strafeEnabled)
            {
                _agent.isStopped = true;
                onStateCompleted.Invoke();
                return;
            }

            if (_enemyVision.IsTargetInActionRange() && strafeEnabled && !_isStrafing)
            {
                StartStrafe();
                return;
            }

            if (_isStrafing)
            {
                UpdateStrafe();
                return;
            }

            ChaseTarget();

            if (_enemyVision.IsTargetInActionRange())
            {
                onStateCompleted.Invoke();
            }
        }

        private void ChaseTarget()
        {
            _agent.isStopped = false;
            _agent.SetDestination(_enemyVision.LastKnownTargetPosition);
        }

        private void StartStrafe()
        {
            var directions = new[]
            {
                transform.forward,
                -transform.forward,
                transform.right,
                -transform.right
            };

            var dir = directions[Random.Range(0, directions.Length)];
            var candidate = transform.position + dir.normalized * strafeDistance;

            if (NavMesh.SamplePosition(candidate, out var hit, 2f, NavMesh.AllAreas))
            {
                _strafeTarget = hit.position;
                _isStrafing = true;
                _agent.SetDestination(_strafeTarget);
            }
            else
            {
                onStateCompleted.Invoke();
            }
        }

        private void UpdateStrafe()
        {
            if (_agent.pathPending) return;

            if (!(_agent.remainingDistance <= _agent.stoppingDistance)) return;
            _isStrafing = false;
            onStateCompleted.Invoke();
        }
    }
}