using System.Collections.Generic;
using System.Linq;
using ModernMalick.Audio;
using ModernMalick.Core.LeanTween;
using ModernMalick.Core.MonoBehaviourExtensions;
using ModernMalick.Timers;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ModernMalick.Player.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviourExtended
    {
        [Header("Components")]
        [SerializeField] private PlayerGround playerGround;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Horizontal")]
        [SerializeField] private float walkSpeed = 10f;
        [SerializeField, Range(0f, 1f)] private float airControlFactor = 1f;
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioClip footstepClip;
        [SerializeField] private AudioClip landedClip;
        
        [Header("Jump")]
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float coyoteTime = 0.3f;
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private bool canDoubleJump = true;
        [SerializeField] private float doubleJumpForce = 15f;
        [SerializeField] private AudioClip doubleJumpClip;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 60f;
        [SerializeField] private float dashDuration = 0.125f;
        [SerializeField] private Cooldown dashCooldown;
        
        [Header("Dash VFX")]
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private float dashFOVIncrease = 20;
        [SerializeField] private float dashTweenTime = 0.1f;
        [SerializeField] private AudioClip dashReadyClip;
        [SerializeField] private AudioClip dashStartClip;
        [SerializeField] private AudioClip dashEndClip;
        [SerializeField] private ParticleSystem dashParticles;
        
        [Header("Tween Animations")]
        [SerializeField] private List<Transform> animatedTransforms;
        [SerializeField] private float swayAmount = 0.025f;
        [SerializeField] private float swaySpeed = 4f;
        [SerializeField] private float maxSwayDistance = 0.1f;
        [SerializeField] private float bobFrequency = 6f;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float verticalTiltAmount = 20f;
        [SerializeField] private float verticalMoveAmount = 0.25f;
        [SerializeField] private float verticalTweenSpeed = 10f;
        
        [Component] private Rigidbody _playerBody;

        private Vector2 _moveInput;
        private Camera _camera;
        private float _currentSpeed;

        private float _coyoteTimeCounter;
        private bool _hasJumped;
        private bool _canDoubleJump;
        
        private float _dashTimer;
        private bool _isDashing;
        
        private float _initialFOV;
        
        private readonly List<Vector3> _initialLocalPositions = new();
        private Vector3 _targetPosition;
        private float _bobTimer;
        private float _bobOffset;
        private float _airborneOffset;
        private int _lastStepIndex = -1;
        
        private new void Awake()
        {
            base.Awake();
            _camera = Camera.main;
            _currentSpeed = walkSpeed;
        }

        private void Start()
        {
            dashCooldown.Start();
            
            foreach (var t in animatedTransforms.Where(t => t != null))
                _initialLocalPositions.Add(t.localPosition);
        }

        private void OnEnable()
        {
            playerGround.OnLanded += OnLanded;
            dashCooldown.OnCooldownFinished += OnDashReady;
        }

        private void OnDisable()
        {            
            playerGround.OnLanded -= OnLanded;
            dashCooldown.OnCooldownFinished -= OnDashReady;
        }

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }
        
        private void Update()
        {
            UpdateCoyoteTime();
            
            if (!_hasJumped && playerGround.IsGrounded())
            {
                _playerBody.linearVelocity = new Vector3(_playerBody.linearVelocity.x, 0, _playerBody.linearVelocity.z);
            }
            
            dashCooldown.Tick(Time.deltaTime);

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;

                if (_dashTimer <= 0f)
                {
                    StopDash();
                }
            }
            
            ApplySway();
            ApplyBob();
            ApplyAirborne();
            ApplyCombinedOffsets();
        }
        
        private void FixedUpdate()
        {
            var input = new Vector3(_moveInput.x, 0f, _moveInput.y);

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            var worldInput = _camera.transform.TransformDirection(input);

            var speed = _currentSpeed;

            if (playerGround && !playerGround.IsGrounded())
            {
                speed *= airControlFactor;
            }

            var targetVelocity = worldInput * speed;

            _playerBody.linearVelocity = new Vector3(targetVelocity.x, _playerBody.linearVelocity.y, targetVelocity.z);
        }
        
        public void SetSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        public void ResetSpeed()
        {
            _currentSpeed = walkSpeed;
        }
        
        public void OnJump(InputValue value)
        {
            if(!value.isPressed || Time.timeScale == 0) return;

            if (playerGround == null)
            {
                Debug.LogError("PlayerJump requires a PlayerGround component.");
                return;
            }
            
            if(playerGround.IsGrounded() || _coyoteTimeCounter > 0)
            {
                PerformJump();
            } else if (canDoubleJump && _hasJumped && _canDoubleJump && !playerGround.IsGrounded())
            {
                PerformDoubleJump();
            }
        }
        
        private void PerformJump()
        {
            _coyoteTimeCounter = 0;
            _hasJumped = true;
            AddJumpForce(jumpForce);
            AudioHelper.TryPlayAudio(audioSource, jumpClip);
        }
        
        private void PerformDoubleJump()
        {
            _playerBody.linearVelocity = new Vector3(_playerBody.linearVelocity.x, 0);
            AddJumpForce(doubleJumpForce);
            _canDoubleJump = false;
            AudioHelper.TryPlayAudio(audioSource, doubleJumpClip);
        }
        
        private void AddJumpForce(float force)
        {
            _playerBody.AddForce(Vector3.up * force, ForceMode.Impulse);
        }
        
        private void OnLanded()
        {
            _coyoteTimeCounter = coyoteTime;
            _hasJumped = false;
            _canDoubleJump = true;
            AudioHelper.TryPlayAudio(audioSource, landedClip);
        }

        private void UpdateCoyoteTime()
        {
            if(playerGround.IsGrounded()) return;
            if (_coyoteTimeCounter > 0)
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
            else
            {
                _coyoteTimeCounter = 0;
            }
        }
        
        public void OnDash(InputValue value)
        {
            if (!dashCooldown.IsReady() || _isDashing) return;

            dashCooldown.Reset();
            dashCooldown.Start();

            _isDashing = true;
            _dashTimer = dashDuration;

            SetSpeed(dashSpeed);
            _playerBody.useGravity = false;
            
            if (cinemachineCamera)
            {
                _initialFOV = cinemachineCamera.Lens.FieldOfView;
                var targetFov = _initialFOV + dashFOVIncrease;
                LeanTween.value(_initialFOV, targetFov, dashTweenTime)
                    .setOnUpdate(fov => { cinemachineCamera.Lens.FieldOfView = fov; });
            }
            
            AudioHelper.TryPlayAudio(audioSource, dashStartClip);
            
            
            if (dashParticles)
            {
                dashParticles.Play();
            }
        }
        
        private void OnDashReady()
        {
            AudioHelper.TryPlayAudio(audioSource, dashReadyClip);
        }
        
        private void StopDash()
        {
            _isDashing = false;

            ResetSpeed();
            _playerBody.useGravity = true;
            
            if (cinemachineCamera)
            {
                LeanTween.value(cinemachineCamera.Lens.FieldOfView, _initialFOV, dashTweenTime)
                    .setOnUpdate(value => { cinemachineCamera.Lens.FieldOfView = value; });
            }
            
            AudioHelper.TryPlayAudio(audioSource, dashEndClip);
            
            if (dashParticles)
            {
                dashParticles.Stop();
            }
        }
        
        private void ApplySway()
        {
            var horizontal = new Vector2(_moveInput.x, _moveInput.y);
            if (horizontal.sqrMagnitude > 1f)
            {
                horizontal.Normalize();
            }

            _targetPosition = new Vector3(
                horizontal.x * swayAmount,
                0f,
                horizontal.y * swayAmount
            );

            _targetPosition = Vector3.ClampMagnitude(_targetPosition, maxSwayDistance);

            for (var i = 0; i < animatedTransforms.Count; i++)
            {
                var t = animatedTransforms[i];
                if (t == null || !t.gameObject.activeSelf) continue;

                t.localPosition = Vector3.Lerp(
                    t.localPosition,
                    _initialLocalPositions[i] + _targetPosition,
                    Time.deltaTime * swaySpeed
                );
            }
        }

        private void ApplyBob()
        {
            var velocity = _playerBody.linearVelocity;
            var horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;

            if (horizontalSpeed < 0.1f || (playerGround && !playerGround.IsGrounded()))
            {
                _bobTimer = 0f;
                _bobOffset = Mathf.Lerp(_bobOffset, 0f, Time.deltaTime * bobFrequency);
                _lastStepIndex = -1;
                return;
            }

            _bobTimer += Time.deltaTime * bobFrequency;

            _bobOffset = Mathf.Sin(_bobTimer) * bobAmplitude;

            var stepIndex = Mathf.FloorToInt(_bobTimer / Mathf.PI);

            if (stepIndex == _lastStepIndex) return;
            _lastStepIndex = stepIndex;

            if (footstepAudioSource)
            {
                var foot = stepIndex % 2 == 0 ? -0.5f : 0.5f;
                footstepAudioSource.panStereo = foot;
                footstepAudioSource.Play();
            }
        }
        
        private void ApplyAirborne()
        {
            var verticalVelocity = _playerBody.linearVelocity.y;
            var normalized = Mathf.Clamp(verticalVelocity / 10f, -1f, 1f);
            _airborneOffset = Mathf.Lerp(_airborneOffset, -normalized * verticalMoveAmount, Time.deltaTime * verticalTweenSpeed);
        }
        
        private void ApplyCombinedOffsets()
        {
            for (var i = 0; i < animatedTransforms.Count; i++)
            {
                var t = animatedTransforms[i];
                if (t == null || !t.gameObject.activeSelf) continue;

                var basePos = _initialLocalPositions[i] + _targetPosition;
                basePos.y += _bobOffset + _airborneOffset;
                t.localPosition = Vector3.Lerp(t.localPosition, basePos, Time.deltaTime * swaySpeed);

                var targetTilt = Quaternion.Euler(-(_airborneOffset / verticalMoveAmount) * verticalTiltAmount, 0f, 0f);
                t.localRotation = Quaternion.Slerp(t.localRotation, targetTilt, Time.deltaTime * verticalTweenSpeed);
            }
        }
    }
}