using UnityEngine;

namespace PlayerScripts
{
    public class InputActionsContainer : MonoBehaviour
    {
        public PlayerInputActions InputActions { get; private set; }
        public static InputActionsContainer Instance { get; private set; }
    
        private void Awake()
        {
            Instance = this;
            InputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            InputActions.Enable();
        }

        private void OnDisable()
        {
            InputActions.Disable();
        }
    }
}