using ModernMalick.Audio;
using Unity.Cinemachine;
using UnityEngine;

namespace ModernMalick.Player.Arsenal
{
    public class BlackHole : MonoBehaviour
    {
        [Header("Pull")]
        [SerializeField] private float radius = 5f;
        [SerializeField] private float pullForce = 20f;
        [SerializeField] private float pullTime = 5f;
        [SerializeField] private LayerMask pullMask;

        [Header("Damage")]
        [SerializeField] private float damageRate = 2f;
        [SerializeField] private int damagePerTick = 1;

        [Header("Release")]
        [SerializeField] private float explosionForce = 15f;
        [SerializeField] private int finalDamage = 1;

        [Header("VFX")] 
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private ParticleSystem blackHoleParticles;
        [SerializeField] private ParticleSystem explosionParticles;
        
        private float _elapsed;
        private float _damageTimer;

        private readonly Collider[] _results = new Collider[100];

        private void FixedUpdate()
        {
            var count = Physics.OverlapSphereNonAlloc(
                transform.position,
                radius,
                _results,
                pullMask
            );

            _elapsed += Time.fixedDeltaTime;
            _damageTimer += Time.fixedDeltaTime;

            var expired = _elapsed >= pullTime;
            var applyTickDamage = _damageTimer >= damageRate;

            for (var i = 0; i < count; i++)
            {
                var rb = _results[i].attachedRigidbody;
                if (!rb) continue;

                if (expired)
                {
                    Release(rb);
                }
                else
                {
                    Pull(rb);

                    if (applyTickDamage)
                        ApplyDamage(rb, damagePerTick);
                }
            }

            if (applyTickDamage)
                _damageTimer = 0f;

            if (!expired) return;
            
            if (impulseSource)
            {
                impulseSource.GenerateImpulse();
            }
            
            AudioManager.TryPlayAudio(audioSource, explosionClip);
            
            if (audioSource)
            {
                audioSource.loop = false;
            }
            
            if (blackHoleParticles)
            {
                blackHoleParticles.gameObject.SetActive(false);
            }
            
            if (explosionParticles)
            {
                explosionParticles.Play();
            }
            
            Destroy(gameObject, 2f);

            enabled = false;
        }

        private void Pull(Rigidbody rb)
        {
            rb.useGravity = false;

            var direction = (transform.position - rb.position).normalized;
            rb.AddForce(direction * pullForce, ForceMode.Acceleration);
        }

        private void Release(Rigidbody rb)
        {
            rb.useGravity = true;

            var dir = (rb.position - transform.position).normalized;
            rb.AddForce(dir * explosionForce, ForceMode.VelocityChange);

            ApplyDamage(rb, finalDamage);
        }

        private static void ApplyDamage(Rigidbody rb, int damage)
        {
            var health = rb.GetComponent<Health.Health>();
            if (health)
                health.ModifyHealth(-damage);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}