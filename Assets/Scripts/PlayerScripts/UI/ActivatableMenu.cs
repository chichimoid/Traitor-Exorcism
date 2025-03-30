using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerScripts
{
    public class ActivatableMenu : MonoBehaviour
    {
        [SerializeField] private GameObject ui;
        [SerializeField] private string inputActionName;
        [SerializeField] private bool lockPlayerMovement;
        [SerializeField] private bool requireHoldingKey;
        [SerializeField] private bool hiddenByDefault;
        
        private InputAction _inputAction;
        private bool _isActive = false;

        private void Start()
        {
            _inputAction = InputActionsContainer.Instance.InputActions.FindAction(inputActionName, true);

            if (hiddenByDefault)
            {
                ui.SetActive(false);
            }
            
            OnEnable();
        }
        private void OnEnable()
        {
            if (!didStart) return;
            _inputAction.performed += HandleInputPerformed;
            if (requireHoldingKey)
            {
                _inputAction.canceled += HandleInputCanceled;
            }
        }

        private void OnDisable()
        {
            _inputAction.performed -= HandleInputPerformed;
            if (requireHoldingKey)
            {
                _inputAction.canceled -= HandleInputCanceled;
            }
        }

        public void Show()
        {
            _isActive = true;
            
            ui.SetActive(true);
            PlayerLocker.Instance.UnlockCursor();
            if (lockPlayerMovement)
            {
                PlayerLocker.Instance.LockMovement();
            }
        }
        
        public void Hide()
        {
            _isActive = false;
            
            ui.SetActive(false);
            PlayerLocker.Instance.LockCursor();
            if (lockPlayerMovement)
            {
                PlayerLocker.Instance.UnlockMovement();
            }
        }
        
        private void HandleInputPerformed(InputAction.CallbackContext context)
        {
            if (!requireHoldingKey && _isActive)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void HandleInputCanceled(InputAction.CallbackContext context)
        {
            Hide();
        }
    }
}