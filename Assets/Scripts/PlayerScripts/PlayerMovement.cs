using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerScripts
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float monsterMultiplier;
        
        private float _multiplier = 1;
    
        private Rigidbody _playerRigidbody;
        private PlayerInputActions _inputActions;
        private InputAction _moveAction;
        private InputAction _sprintAction;
        
        public bool CanMove { get; set; } = true;
        public bool CanSprint { get; set; } = true;
        
        private void Start()
        {
            if (!IsOwner) return;
            
            _inputActions = GetComponent<InputActionsContainer>().InputActions;
            _moveAction = _inputActions.Player.Move;
            _sprintAction = _inputActions.Player.Sprint;
            
            _playerRigidbody = GetComponent<Rigidbody>();

            var playerStamina = GetComponent<PlayerStamina>();
            playerStamina.OnPlayedTired += () => CanSprint = false;
            playerStamina.OnPlayerUnTired += () => CanSprint = true;
        }
    
        private void FixedUpdate()
        {
            if (!IsOwner) return;
            if (CanMove)
            {
                var moveVector = _moveAction.ReadValue<Vector2>();
                bool isSprinting = _sprintAction.IsPressed() && CanSprint;
                if (moveVector.sqrMagnitude > float.Epsilon && isSprinting)
                {
                    OnPlayerIsRunning?.Invoke();
                }
                
                MovePlayer(moveVector, isSprinting);
            }
        }

        private void MovePlayer(Vector2 moveVector, bool isSprinting)
        {
            _playerRigidbody.linearVelocity = transform.TransformDirection(new Vector3(
                moveVector.x * (isSprinting ? sprintSpeed : moveSpeed) * _multiplier,
                _playerRigidbody.linearVelocity.y, 
                moveVector.y * (isSprinting ? sprintSpeed : moveSpeed) * _multiplier));
            
            // Alternative movement system (if player has no RigidBody)
            //var moveVector = playerInputActions.Player.Move.ReadValue<Vector2>();
            //transform.Translate( moveSpeed * new Vector3(moveVector.x, 0f, moveVector.y), Space.Self);
        }

        public void ToMonsterMultiplier()
        {
            _multiplier = monsterMultiplier;
        }
        public delegate void OnPlayerIsRunningDelegate();
        public event OnPlayerIsRunningDelegate OnPlayerIsRunning;
    }
}