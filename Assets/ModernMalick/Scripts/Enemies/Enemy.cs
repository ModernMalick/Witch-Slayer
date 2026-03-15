using System.Collections.Generic;
using ModernMalick.Core.MonoBehaviourExtensions;
using ModernMalick.Core.StateMachine;
using ModernMalick.Enemies.Actions;
using UnityEngine;

namespace ModernMalick.Enemies
{
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(EnemyVision))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyAction))]
    [RequireComponent(typeof(Health.Health))]
    [RequireComponent(typeof(Animator))]
    public class Enemy : MonoBehaviourExtended
    {
        [SerializeField] private List<GameObject> deathSpeawnPrefabs;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private List<AudioClip> hurtSounds;
        
        [Component] private StateMachine _stateMachine;
        [Component] private EnemyVision _vision;
        [Component] private EnemyMovement _movementState;
        [Component] private EnemyAction _actionState;
        [Component] private Health.Health _health;
        [Component] private Animator _animator;
        
        private void Start()
        {
            SetMovementState();
        }

        private void OnEnable()
        {
            _movementState.onStateCompleted += SetActionState;
            _actionState.onStateCompleted += SetMovementState;
            _health.OnHealthDecreased += OnHealthDecreased;
            _health.OnHealthDepleted += OnHealthDepleted;
        }
        
        private void OnDisable()
        {
            _movementState.onStateCompleted -= SetActionState;
            _actionState.onStateCompleted -= SetMovementState;
            _health.OnHealthDecreased -= OnHealthDecreased;
            _health.OnHealthDepleted -= OnHealthDepleted;
        }

        private void SetActionState()
        {
            _stateMachine.SetState(_actionState);
        }

        private void SetMovementState()
        {
            _stateMachine.SetState(_movementState);
        }

        private void OnHealthDecreased(int delta)
        {
            _animator.SetTrigger("Hurt");
            
            if (audioSource && hurtSounds != null && hurtSounds.Count > 0)
            {
                var clip = hurtSounds[Random.Range(0, hurtSounds.Count)];
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void OnHealthDepleted()
        {
            if (deathSpeawnPrefabs != null)
            {
                foreach (var deathSpeawnPrefab in deathSpeawnPrefabs)
                {
                    Instantiate(deathSpeawnPrefab, transform.position, Quaternion.identity);
                }
            }
            
            Destroy(gameObject);
        }
    }
}