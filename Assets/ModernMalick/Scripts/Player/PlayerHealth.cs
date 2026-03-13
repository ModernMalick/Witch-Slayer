using ModernMalick.Audio;
using ModernMalick.Core.MonoBehaviourExtensions;
using Unity.Cinemachine;
using UnityEngine;

namespace ModernMalick.Player
{
    [RequireComponent(typeof(Health.Health))]
    public class PlayerHealth : MonoBehaviourExtended
    {
        [Header("Audio")]
        [SerializeField] private AudioSource healthAudio;
        [SerializeField] private AudioClip healClip;
        [SerializeField] private AudioClip hurtClip;
        
        [Header("VFX")]
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private ParticleSystem healParticles;
        [SerializeField] private ParticleSystem hurtParticles;
        
        [Component] private Health.Health _health;
        
        private void OnEnable()
        {
            _health.OnHealthDecreased += OnHurt;
            _health.OnHealthIncreased += OnHeal;
            _health.OnHealthDepleted += OnDeath;
        }

        private void OnDisable()
        {
            _health.OnHealthDecreased -= OnHurt;
            _health.OnHealthIncreased -= OnHeal;
            _health.OnHealthDepleted -= OnDeath;
        }

        private void OnHurt(int health)
        {
            AudioManager.TryPlayAudio(healthAudio, hurtClip);
            
            if (hurtParticles)
            {
                hurtParticles.Play();
            }

            if (impulseSource)
            {
                impulseSource.GenerateImpulse();
            }
        }
        
        private void OnHeal(int health)
        {
            AudioManager.TryPlayAudio(healthAudio, healClip);
            if (healParticles)
            {
                healParticles.Play();
            }
        }

        private void OnDeath()
        {
            
        }
    }
}