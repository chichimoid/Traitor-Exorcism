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
        public Transform Player { get; private set; } = null;

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
            InteractRpc(interactor.GetComponent<NetworkObject>());
        }

        [Rpc(SendTo.Everyone)]
        private void InteractRpc(NetworkObjectReference parentReference)
        {
            _canInteract = false;
            _objectRigidbody.isKinematic = true;
            _objectCollider.isTrigger = true;

            parentReference.TryGet(out NetworkObject parent);
            if (parent.GetComponent<NetworkPlayer>().HeldObjSecond is not null)
            {
                FollowTransformManager.Instance.Follow(transform, parent.GetComponent<PlayerInteract>().HoldPointTransformSecond);
                return;
            }
            FollowTransformManager.Instance.Follow(transform, parent.GetComponent<PlayerInteract>().HoldPointTransformMain);
            
            Player = parent.transform;
        }

        public void Drop()
        {
            DropRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void DropRpc()
        {
            Player = null;

            FollowTransformManager.Instance.Unfollow(transform);
            
            _objectRigidbody.isKinematic = false;
            _objectCollider.isTrigger = false;
            _canInteract = true;
        }
    }
}
