using ModernMalick.Audio;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using ModernMalick.Core.LeanTween;

namespace ModernMalick.Player.Arsenal
{
    public class BlackHole : MonoBehaviour
    {
        [Header("Pull")]
        [SerializeField] private float radius = 5f;
        [SerializeField] private float pullTime = 5f;
        [SerializeField] private LayerMask pullMask;

        [Header("Damage")]
        [SerializeField] private float damageRate = 2f;
        [SerializeField] private int damagePerTick = 1;

        [Header("Release")]
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
        private readonly HashSet<Transform> _pulled = new();

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
                var t = _results[i].transform;

                if (!_pulled.Contains(t) && !expired)
                {
                    StartPull(t);
                    _pulled.Add(t);
                }

                if (applyTickDamage)
                    ApplyDamage(t.gameObject, damagePerTick);

                if (expired)
                    ApplyDamage(t.gameObject, finalDamage);
            }

            if (applyTickDamage)
                _damageTimer = 0f;

            if (!expired) return;

            if (impulseSource)
                impulseSource.GenerateImpulse();

            AudioManager.TryPlayAudio(audioSource, explosionClip);

            if (audioSource)
                audioSource.loop = false;

            if (blackHoleParticles)
                blackHoleParticles.gameObject.SetActive(false);

            if (explosionParticles)
                explosionParticles.Play();

            Destroy(gameObject, 2f);
            enabled = false;
        }

        private void StartPull(Transform target)
        {
            var pos = transform.position;
            pos.y = target.position.y;

            LeanTween.move(target.gameObject, pos, pullTime)
                .setEase(LeanTweenType.easeInQuad);
        }

        private static void ApplyDamage(GameObject obj, int damage)
        {
            var health = obj.GetComponent<Health.Health>();
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