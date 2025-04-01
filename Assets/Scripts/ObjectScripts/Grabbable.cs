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
            GrabbableSetFollowTransformRpc(interactor.GetComponent<NetworkObject>());
        }

        [Rpc(SendTo.Everyone)]
        private void GrabbableSetFollowTransformRpc(NetworkObjectReference parentReference)
        {
            _canInteract = false;
            
            _objectRigidbody.isKinematic = true;
            _objectCollider.isTrigger = true;
            
            parentReference.TryGet(out var parent);
            FollowTransformManager.Instance.Follow(transform, parent.GetComponent<PlayerInteract>().HoldPointTransform);
        }

        public void Drop()
        {
            DropRpc();
        }
        
        [Rpc(SendTo.Everyone)]
        private void DropRpc()
        {
            FollowTransformManager.Instance.Unfollow(transform);
            
            _objectRigidbody.isKinematic = false;
            _objectCollider.isTrigger = false;
            
            _canInteract = true;
        }
    }
}
