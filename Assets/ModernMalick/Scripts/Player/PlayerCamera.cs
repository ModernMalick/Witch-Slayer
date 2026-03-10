using UnityEngine;
using UnityEngine.InputSystem;

namespace ModernMalick.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform playerBody;
        [SerializeField] private float maxVerticalAngle = 45f;

        private Vector3 _cameraOffset;
        private Vector2 _lookInput;
        private Vector2 _currentVelocity;
        private float _yaw;
        private float _pitch;
        
        private void Awake()
        {
            _cameraOffset = transform.localPosition - playerBody.localPosition;
            var euler = playerBody.transform.localRotation.eulerAngles;
            _yaw = euler.y;
            _pitch = transform.localRotation.eulerAngles.x;
        }
        
        public void OnLook(InputValue value)
        {
            _lookInput = value.Get<Vector2>();
        }

        private void Update()
        {
            transform.localPosition = playerBody.transform.localPosition + _cameraOffset;
        }

        private void LateUpdate()
        {
            _yaw += _lookInput.x * Time.deltaTime;
            _pitch -= _lookInput.y * Time.deltaTime;
            
            _pitch = Mathf.Clamp(_pitch, -maxVerticalAngle, maxVerticalAngle);

            transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}