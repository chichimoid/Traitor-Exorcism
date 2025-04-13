using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace PlayerScripts.UI
{
    public class ActivatableMenu : MonoBehaviour
    {
        [SerializeField] private GameObject uiObject;
        [SerializeField] private string inputActionName;
        [SerializeField] private bool lockPlayerActions;
        [SerializeField] private bool requireHoldingKey;
        [SerializeField] private bool hiddenByDefault;
        
        private InputAction _inputAction;
        private bool _isActive = false;

        private void Start()
        {
            _inputAction = InputActionsContainer.Instance.InputActions.FindAction(inputActionName, true);

            if (hiddenByDefault)
            {
                uiObject.SetActive(false);
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
            
            uiObject.SetActive(true);
            PlayerLocker.Instance.UnlockCursor();
            if (lockPlayerActions)
            {
                PlayerLocker.Instance.LockMovement();
                PlayerLocker.Instance.LockActions();
            }
        }
        
        public void Hide()
        {
            _isActive = false;
            
            uiObject.SetActive(false);
            PlayerLocker.Instance.LockCursor();
            if (lockPlayerActions)
            {
                PlayerLocker.Instance.UnlockMovement();
                PlayerLocker.Instance.UnlockActions();
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