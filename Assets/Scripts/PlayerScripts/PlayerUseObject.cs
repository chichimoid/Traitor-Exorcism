using ObjectScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace PlayerScripts
{
    public class PlayerUseObject : NetworkBehaviour
    {
        private NetworkPlayer _networkPlayer;
        private PlayerInputActions _inputActions;
        private InputAction _use;

        private void Start()
        {
            if (!IsOwner) return;
            
            _networkPlayer = GetComponent<NetworkPlayer>();
            _inputActions = GetComponent<InputActionsContainer>().InputActions;
            
            _use = _inputActions.Player.Use;
            
            OnEnable();
        }
        
        private void OnEnable()
        {
            if (!IsOwner || !didStart) return;
            
            _use.performed += Use;
        }

        private void OnDisable()
        {
            if (!IsOwner) return;
            
            _use.performed -= Use;
        }

        private void Use(InputAction.CallbackContext context)
        {
            if (_networkPlayer.HeldObjMain is IUsable usable) {
                usable.Use();
            }
        }
    }
}
