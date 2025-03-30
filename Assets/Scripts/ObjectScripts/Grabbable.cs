using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace ObjectScripts
{
    public abstract class Grabbable : NetworkBehaviour, IGrabbable
    {
        [SerializeField] private GrabbableSO grabbableSO;
    
        private Rigidbody _objectRigidbody;
        private Collider _objectCollider;
        private bool _canInteract = true;
    
        private void Awake()
        {
            _objectRigidbody = GetComponent<Rigidbody>();
            _objectCollider = GetComponent<Collider>();
        }

        public bool CanInteract()
        {
            return _canInteract;
        }
        
        public void Interact(Transform interactor)
        {
            GrabbableSetFollowTransformServerRpc(interactor.GetComponent<NetworkObject>());
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GrabbableSetFollowTransformServerRpc(NetworkObjectReference parentReference)
        {
            GrabbableSetFollowTransformClientRpc(parentReference);
        }

        [ClientRpc]
        private void GrabbableSetFollowTransformClientRpc(NetworkObjectReference parentReference)
        {
            _canInteract = false;
            _objectRigidbody.isKinematic = true;
            _objectCollider.isTrigger = true;
        
            parentReference.TryGet(out NetworkObject parent);
            FollowTransformManager.Instance.Follow(transform, parent.GetComponent<PlayerInteract>().HoldPointTransform);
        }

        public void Drop()
        {
            DropServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void DropServerRpc()
        {
            DropClientRpc();
        }

        [ClientRpc]
        private void DropClientRpc()
        {
            _objectRigidbody.isKinematic = false;
            _objectCollider.isTrigger = false;
            
            FollowTransformManager.Instance.Unfollow(transform);
            _canInteract = true;
        }
    }
}
