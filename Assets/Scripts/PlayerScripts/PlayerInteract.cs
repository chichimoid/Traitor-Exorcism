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
        [SerializeField] private Transform holdPointTransformMain;
        [SerializeField] private Transform holdPointTransformSecond;


        private NetworkPlayer _networkPlayer;
        private PlayerInputActions _inputActions;
        private InputAction _interact;
        private InputAction _drop;
        private InputAction _swap;
        private PlayerRayCaster _rayCaster;
        
        public float InteractionRange => interactionRange;
        public Transform HoldPointTransformMain => holdPointTransformMain;
        public Transform HoldPointTransformSecond => holdPointTransformSecond;


        private void Start()
        {
            if (!IsOwner) return;

            _networkPlayer = GetComponent<NetworkPlayer>();
            _inputActions = GetComponent<InputActionsContainer>().InputActions;
            _rayCaster = GetComponent<PlayerRayCaster>();

            _interact = _inputActions.Player.Interact;
            _drop = _inputActions.Player.Drop;
            _swap = _inputActions.Player.Swap;

            OnEnable();
        }
        private void OnEnable()
        {
            if (!IsOwner || !didStart) return;

            _interact.performed += Interact;
            _drop.performed += Drop;
            _swap.performed += Swap;
        }

        private void OnDisable()
        {
            if (!IsOwner) return;

            _interact.performed -= Interact;
            _drop.performed -= Drop;
            _swap.performed -= Swap;
        }

        private void Interact(InputAction.CallbackContext context)
        {
            Debug.Log("Interact");
            if (_rayCaster.ViewPointRayCast(out var hit, interactionRange))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
                {
                    if (interactable is Grabbable grabbable)
                    {
                        if (_networkPlayer.HeldObjMain is not null && _networkPlayer.HeldObjSecond is not null)
                        {
                            Debug.Log("Hands full");
                            return;
                        }
                        if (_networkPlayer.HeldObjMain is not null && _networkPlayer.HeldObjSecond is null)
                        {
                            _networkPlayer.HeldObjSecond = grabbable;
                            grabbable.Interact(transform);

                        }
                        if (_networkPlayer.HeldObjMain is null)
                        {
                            _networkPlayer.HeldObjMain = grabbable;
                            grabbable.Interact(transform);
                        }

                    }
                    else
                    {
                        interactable.Interact(transform);
                    }
                }
            }
        }
        private void Swap(InputAction.CallbackContext context)
        {
            if (_networkPlayer.HeldObjMain is null || _networkPlayer.HeldObjSecond is null) return;
            var firstCoords = holdPointTransformMain.position;
            var firstRotation = holdPointTransformMain.rotation;
            holdPointTransformMain.SetPositionAndRotation(holdPointTransformSecond.position, holdPointTransformSecond.rotation);
            holdPointTransformSecond.SetPositionAndRotation(firstCoords, firstRotation);
            var temp = _networkPlayer.HeldObjMain;
            _networkPlayer.HeldObjMain = _networkPlayer.HeldObjSecond;
            _networkPlayer.HeldObjSecond = temp;
        }
        private void Drop(InputAction.CallbackContext context)
        {
            _networkPlayer.HeldObjMain?.Drop();
            if (_networkPlayer.HeldObjSecond is not null)
            {
                _networkPlayer.HeldObjSecond?.Drop();
                _networkPlayer.HeldObjSecond = null;

                //FollowTransformManager.Instance.Unfollow(_networkPlayer.HeldObjSecond.ReturnTransform());
                //FollowTransformManager.Instance.Follow(_networkPlayer.HeldObjSecond.ReturnTransform(), holdPointTransformMain);
            }
            _networkPlayer.HeldObjMain = null;
        }
    }
}
