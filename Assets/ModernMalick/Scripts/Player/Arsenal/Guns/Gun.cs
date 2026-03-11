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
        [SerializeField] private GameObject mesh;
        
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

        [Header("Ammo")]
        [SerializeField] private bool useAmmo;
        [SerializeField] private int magazineSize;
        [SerializeField] private int startingAmmo;
        [SerializeField] private float reloadTime;
        [SerializeField] private bool infiniteReserveAmmo;
        [SerializeField] private AmmoData ammoData;
        
        [Header("Animations")]
        [SerializeField] private Vector3 recoilPosition = new(0, 0, -0.1f);
        [SerializeField] private Vector3 recoilRotation = new(-20, 0, 0);
        [SerializeField] private float recoilStartTime = 0.1f;
        [SerializeField] private Vector3 reloadRotation = new(45f, 0, 0);
        [SerializeField] private Vector3 reloadPosition = new(0, -1, 0);
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
        [SerializeField] private AudioClip reloadStartClip;
        [SerializeField] private AudioClip reloadEndClip;
        
        [Header("UI")]
        [SerializeField] private Crosshair crosshairPrefab;
        [SerializeField] private Sprite icon;
        [SerializeField] private GunUI gunUIPrefab;
        
        private Camera _playerCamera;
        private float _lastAttackTime;
        
        private int _currentAmmo;
        private bool _isReloading;

        private Crosshair _crosshair;
        private GunUI _gunUI;
        
        public int CurrentAmmo
        {
            get => _currentAmmo;
            set
            {
                _currentAmmo = Mathf.Clamp(value, 0, magazineSize);
                OnCurrentAmmoChanged.Invoke(_currentAmmo);
            }
        }

        public event Action OnShotFired = delegate { };
        public event Action<RaycastHit> OnPelletHit = delegate { };
        public event Action<int> OnCurrentAmmoChanged = delegate { };
        public event Action<float> OnReloadStarted = delegate { };
        public event Action OnReloadEnded = delegate { };
        public event Action<float> OnReloadProgressChanged = delegate { };
        
        private void Awake()
        {
            Hide();
            _playerCamera = Camera.main;
            CurrentAmmo = startingAmmo;
            
            if (ammoData)
            {
                ammoData.Initialize();
            }
        }

        private void OnEnable()
        {
            Show();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _isReloading = false;
            Hide();
        }

        private bool IsInCooldown()
        {
            return Time.time < _lastAttackTime + 1f / attackRate;
        }

        private bool CanConsume() => CurrentAmmo > 0 && !_isReloading;

        public bool TryAttack()
        {
            if (IsInCooldown()) return false;
            
            if (useAmmo)
            {
                if (!CanConsume())
                {
                    Reload();
                    return false;
                }
                Consume();
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
                OnPelletHit.Invoke(hit);
                HitVFX(hit);
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
                    if(hit.collider != null)
                    {
                        Health.Health.TryModifyHealth(hit.collider.gameObject, -damage);
                    }
                    Destroy(projectile);
                });
        }

        private void Consume()
        {
            if (_currentAmmo <= 0 || _isReloading) return;
            CurrentAmmo--;
            if (reloadTime <= 0 && _currentAmmo <= 0)
            {
                EndReload();
            }
        }

        public void Reload()
        {
            if (_isReloading || CurrentAmmo >= magazineSize || (!infiniteReserveAmmo && ammoData.CurrentReserveAmmo <= 0)) return;
            if(reloadTime > 0f)
            {
                StartCoroutine(ReloadRoutine());
            }
            else
            {
                EndReload();
            }
            
            ResetTweens();
            
            LeanTween.moveLocal(mesh, reloadPosition, reloadTime / 2).setLoopPingPong(1);
            LeanTween.rotateLocal(mesh, reloadRotation, reloadTime / 2).setLoopPingPong(1);
            
            AudioHelper.TryPlayAudio(audioSource, reloadStartClip);
        }

        private IEnumerator ReloadRoutine()
        {
            _isReloading = true;
            OnReloadStarted.Invoke(reloadTime);

            var elapsed = 0f;
            while (elapsed < reloadTime)
            {
                elapsed += Time.deltaTime;
                var percentage = Mathf.Clamp01(elapsed / reloadTime);
                OnReloadProgressChanged.Invoke(percentage);
                
                yield return null;
            }

            EndReload();
        }

        private void EndReload()
        {
            if (infiniteReserveAmmo)
            {
                CurrentAmmo = magazineSize;
            }
            else
            {
                var needed = magazineSize - CurrentAmmo;
                var taken = ammoData.Consume(needed);

                CurrentAmmo += taken;
            }
            _isReloading = false;
            
            OnReloadEnded.Invoke();
            
            AudioHelper.TryPlayAudio(audioSource, reloadEndClip);
        }
        
        private void Show()
        {
            LeanTween.moveLocalY(mesh, 0, selectionTime);
            if(_crosshair) _crosshair.gameObject.SetActive(true);
            if(_gunUI) _gunUI.SetSelected(true);
        }

        private void Hide()
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
            OnReloadStarted += time => _crosshair.AnimateReload(time);
        }

        public void CreateUI(RectTransform arsenalRoot)
        {
            _gunUI = Instantiate(gunUIPrefab, arsenalRoot);
            if (icon) _gunUI.SetIcon(icon);
            _gunUI.OnCurrentAmmoChanged(CurrentAmmo);
            OnCurrentAmmoChanged += _gunUI.OnCurrentAmmoChanged;
            if (infiniteReserveAmmo)
            {
                _gunUI.OnReserveAmmoChanged(magazineSize);
            }
            else if (ammoData)
            {
                _gunUI.OnReserveAmmoChanged(ammoData.CurrentReserveAmmo);
                ammoData.OnCurrentReserveAmmoChanged += _gunUI.OnReserveAmmoChanged;
            }
        }
    }
}
