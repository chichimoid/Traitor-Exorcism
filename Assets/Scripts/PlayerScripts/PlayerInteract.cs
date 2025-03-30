using ObjectScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace PlayerScripts
{
    public class PlayerInteract : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private float interactionRange = 3f;
        [Header("References")]
        [SerializeField] private Transform holdPointTransform;
    
        private NetworkPlayer _networkPlayer;
        private PlayerInputActions _inputActions;
        private InputAction _interact;
        private InputAction _drop;
        private PlayerRayCaster _rayCaster;
        
        public float InteractionRange => interactionRange;
        public Transform HoldPointTransform => holdPointTransform;

        private void Start()
        {
            if (!IsOwner) return;
            
            _networkPlayer = GetComponent<NetworkPlayer>();
            _inputActions = GetComponent<InputActionsContainer>().InputActions;
            _rayCaster = GetComponent<PlayerRayCaster>();
            
            _interact = _inputActions.Player.Interact;
            _drop = _inputActions.Player.Drop;
            
            OnEnable();
        }
        
        private void OnEnable()
        {
            if (!IsOwner || !didStart) return;
            
            _interact.performed += Interact;
            _drop.performed += Drop;
        }

        private void OnDisable()
        {
            if (!IsOwner) return;
            
            _interact.performed -= Interact;
            _drop.performed -= Drop;
        }
    
        private void Interact(InputAction.CallbackContext context)
        {
            Debug.Log("Interact");
            if (_rayCaster.ViewPointRayCast(out RaycastHit hit, interactionRange)) 
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
                {
                    if (interactable is IGrabbable grabbable)
                    {
                        if (_networkPlayer.HeldObj != null)
                        {
                            Debug.Log("Hands full");
                            return;
                        }
                        grabbable.Interact(transform);
                        _networkPlayer.HeldObj = grabbable;
                    }
                    else
                    {
                        interactable.Interact(transform);
                    }
                }
            }
        }

        private void Drop(InputAction.CallbackContext context)
        {
            Debug.Log("Drop");
            
            _networkPlayer.HeldObj?.Drop();
            _networkPlayer.HeldObj = null;
        }
    }
}
