using System;
using System.Collections;
using System.Collections.Generic;
using ModernMalick.Audio;
using ModernMalick.Core.LeanTween;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ModernMalick.Player.Arsenal.Guns
{
    public class Gun : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform originPoint;
        [SerializeField] private AmmoData ammoData;
        
        [Header("Parameters")]
        [SerializeField] private int damage = 10;
        [SerializeField] public float attackRate = 1f;
        [field: SerializeField] public bool IsAutomatic { get; private set; }
        [SerializeField] private float range = 100;
        [SerializeField] private LayerMask mask;
        
        [Header("Pellets")]
        [SerializeField] private GameObject pelletPrefab;
        [SerializeField] private float pelletSpeed = 100;
        [SerializeField] private int pelletCount = 1;
        [SerializeField] private float pelletsAngle;
        
        [Header("Animations")]
        [SerializeField] private GameObject mesh;
        [SerializeField] private Vector3 recoilPosition = new(0, 0, -0.1f);
        [SerializeField] private Vector3 recoilRotation = new(-20, 0, 0);
        [SerializeField] private float recoilStartTime = 0.1f;
        [SerializeField] private float selectionTime = 0.25f;
        [SerializeField] private float deselectionY = -1f;
        [field: SerializeField] public Transform RightGrip { get; private set; }
        [field: SerializeField] public Transform LeftGrip { get; private set; }

        [Header("VFX")]
        [SerializeField] private List<GameObject> impactObjects;
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private ParticleSystem muzzleParticles;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shotClip;
        
        [Header("UI")]
        [SerializeField] private Crosshair crosshairPrefab;
        [SerializeField] private Sprite icon;
        [SerializeField] private GunUI gunUIPrefab;
        
        private Camera _playerCamera;
        private float _lastAttackTime;

        private Crosshair _crosshair;
        private GunUI _gunUI;

        public event Action OnShotFired = delegate { };
        public event Action<RaycastHit> OnPelletHit = delegate { };
        
        public void Initialize()
        {
            _playerCamera = Camera.main;
            
            if (ammoData)
            {
                ammoData.Initialize();
            }
        }

        private void OnEnable()
        {
            Show();
            _lastAttackTime = Time.time - 1 / attackRate;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            Hide();
        }

        private bool IsInCooldown()
        {
            return Time.time < _lastAttackTime + 1f / attackRate;
        }

        public bool TryAttack()
        {
            if (IsInCooldown()) return false;
            
            if (!ammoData.infiniteAmmo)
            {
                if (!ammoData.CanConsume())
                {
                    return false;
                }
                ammoData.Consume();
            }
            
            ExecuteAttack();
            
            _lastAttackTime = Time.time;
            
            return true;
        }
        
        private void ExecuteAttack()
        {
            for (var i = 0; i < pelletCount; i++)
            {                
                var spread = Random.insideUnitCircle * Mathf.Tan(pelletsAngle * Mathf.Deg2Rad);
                var direction = (_playerCamera.transform.forward +
                                 _playerCamera.transform.right * spread.x +
                                 _playerCamera.transform.up * spread.y).normalized;

                var rayOrigin = _playerCamera.transform.position;
                var start = originPoint.position;
                var end = rayOrigin + direction * range;
                
                if (Physics.Raycast(rayOrigin, direction, out var hit, range, mask))
                {
                    end = hit.point;
                }

                CreatePellet(start, end, hit);
            }
            OnShotFired.Invoke();
            ShotVFX();
        }
        
        private void CreatePellet(Vector3 start, Vector3 end, RaycastHit hit)
        {
            var projectile = Instantiate(pelletPrefab, start, Quaternion.LookRotation(end - start));
            LeanTween.move(projectile.gameObject, end, 0.1f)
                .setSpeed(pelletSpeed)
                .setOnComplete(() =>
                {
                    OnPelletHit.Invoke(hit);
                    HitVFX(hit);
                    if(hit.collider != null)
                    {
                        Health.Health.TryModifyHealth(hit.collider.gameObject, -damage);
                    }
                    Destroy(projectile);
                });
        }
        
        private void Show()
        {
            LeanTween.moveLocalY(mesh, 0, selectionTime);
            if(_crosshair) _crosshair.gameObject.SetActive(true);
            if(_gunUI) _gunUI.SetSelected(true);
        }

        public void Hide()
        {
            LeanTween.moveLocalY(mesh, deselectionY, 0);
            if(_crosshair) _crosshair.gameObject.SetActive(false);
            if(_gunUI) _gunUI.SetSelected(false);
        }
        
        private void ResetTweens()
        {
            LeanTween.cancel(mesh);
            LeanTween.moveLocal(mesh, Vector3.zero, 0);
            LeanTween.rotateLocal(mesh, Vector3.zero, 0);
        }
        
        private void ShotVFX()
        {
            AudioHelper.TryPlayAudio(audioSource, shotClip);
            
            ResetTweens();
            
            var resetTime = 1 / attackRate - recoilStartTime;

            LeanTween.moveLocal(mesh, recoilPosition, recoilStartTime)
                .setOnComplete(() =>
                {
                    LeanTween.moveLocal(mesh, Vector3.zero, resetTime);
                });
            
            LeanTween.rotateLocal(mesh, recoilRotation, recoilStartTime)
                .setOnComplete(() =>
                {
                    LeanTween.rotateLocal(mesh, Vector3.zero, resetTime);
                });

            if (muzzleParticles)
            {
                muzzleParticles.Play();
            }

            if (impulseSource)
            {
                impulseSource.GenerateImpulse();
            }
        }
        
        private void HitVFX(RaycastHit hit)
        {
            if (hit.collider == null) return;
            
            foreach (var impactObject in impactObjects)
            {
                Instantiate(impactObject, hit.point, Quaternion.LookRotation(-hit.normal));
            }
        }

        public void CreateCrosshair(RectTransform crosshairRoot)
        {
            _crosshair = Instantiate(crosshairPrefab, crosshairRoot);
            OnShotFired += () => _crosshair.AnimateShot(attackRate);
        }

        public void CreateUI(RectTransform arsenalRoot)
        {
            _gunUI = Instantiate(gunUIPrefab, arsenalRoot);
            if (icon) _gunUI.SetIcon(icon);

            if (ammoData.infiniteAmmo)
            {
                _gunUI.SetInfiniteAmmo();
                return;
            }
            
            _gunUI.OnCurrentAmmoChanged(ammoData.CurrentAmmo);
            ammoData.OnAmmoChanged += _gunUI.OnCurrentAmmoChanged;
        }
    }
}
