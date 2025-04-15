using PlayerScripts.UI;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerLocker : MonoBehaviour
    {
        private PlayerInteract _playerInteract;
        private PlayerAttack _playerAttack;
        private PlayerMovement _playerMovement;
        private PlayerRotation _playerRotation;
        private Rigidbody _rigidbody;
        public static PlayerLocker Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _playerInteract = GetComponent<PlayerInteract>();
            _playerAttack = GetComponent<PlayerAttack>();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerRotation = GetComponent<PlayerRotation>();
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _playerRotation.CanRotate = true;
        }
        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _playerRotation.CanRotate = false;
        }

        public void LockMovement()
        {
            _playerMovement.CanMove = false;
        }
        public void UnlockMovement()
        {
            _playerMovement.CanMove = true;
        }
        
        public void LockRotation()
        {
            _playerRotation.CanRotate = false;
        }
        public void UnlockRotation()
        {
            _playerRotation.CanRotate = true;
        }

        public void LockPhysics()
        {
            _rigidbody.isKinematic = true;
        }
        public void UnlockPhysics()
        {
            _rigidbody.isKinematic = false;
        }

        public void LockActions()
        {
            _playerInteract.enabled = false;
            _playerAttack.enabled = false;
        }
        public void UnlockActions()
        {
            _playerInteract.enabled = true;
            _playerAttack.enabled = true;
        }
        
        public void LockActivatableUI()
        {
            PlayerUI.Instance.EmoteWheelUI.Hide();
            PlayerUI.Instance.PauseMenuUI.enabled = false;
            PlayerUI.Instance.PauseMenuUI.Hide();
            PlayerUI.Instance.EmoteWheelUI.enabled = false;
        }
    }
}