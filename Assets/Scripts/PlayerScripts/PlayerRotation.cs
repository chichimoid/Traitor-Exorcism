using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerScripts
{
    public class PlayerRotation : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private float mouseSensitivity;
        [SerializeField] private float maxVerticalAngle;
        [SerializeField] private float minVerticalAngle;
        [SerializeField] private Transform head;
    
        private Rigidbody _playerRigidbody;
        private PlayerInputActions _inputActions;
        private InputAction _lookAction;
            
        public bool CanRotate { get; set; } = true;
        
        private void Start()
        {
            if (!IsOwner) return;
            
            _inputActions = GetComponent<InputActionsContainer>().InputActions;
            _lookAction = _inputActions.Player.Look;
            
            _playerRigidbody = GetComponent<Rigidbody>();
            
            // Some variable adjustments (for neater numbers in the editor parameters)
            mouseSensitivity /= 10f;
        }
        private void Update()
        {
            if (!IsOwner) return;
            
            if (CanRotate)
            {
                var lookVector = _lookAction.ReadValue<Vector2>();
                RotatePlayerY(lookVector.x * mouseSensitivity);
                RotatePlayerHeadX(lookVector.y * mouseSensitivity);
            }
        }
        
        private void RotatePlayerY(float angle)
        {
            _playerRigidbody.MoveRotation(_playerRigidbody.rotation * Quaternion.Euler(0, angle, 0));
            
            // Alternative movement system (if player has no RigidBody)
            //transform.Rotate(0, angle, 0);
        }
        
        private void RotatePlayerHeadX(float angle)
        {
            var euler = head.eulerAngles;
            euler.x -= angle;
            // Without the following eulerAngles teleport the camera when out of (-180; 180) interval
            switch (euler.x)
            {
                case > 180:
                    euler.x -= 360;
                    break;
                case < -180:
                    euler.x += 360;
                    break;
            }
        
            euler.x = Math.Clamp(euler.x, minVerticalAngle, maxVerticalAngle);
            head.eulerAngles = euler;
        }
    }
}