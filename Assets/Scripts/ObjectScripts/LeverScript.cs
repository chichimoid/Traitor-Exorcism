using UnityEngine;
using Unity.Netcode;
using UnityEditorInternal;

namespace ObjectScripts
{
    public class LeverScript : NetworkBehaviour, IInteractable
    {
        [SerializeField] private Transform leverHandle;
        [SerializeField] private float rotationAngle = 90f; 
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private Door door;
        [SerializeField] private AudioSource audioSource;

        private bool _isUp = false;
        private Quaternion _downRotation;
        private Quaternion _upRotation;
        private bool _isMoving = false;
        private bool _hasMoved = false;

        private void Start()
        {
            _downRotation = leverHandle.localRotation;
            _upRotation = Quaternion.Euler(_downRotation.eulerAngles.x - rotationAngle, _downRotation.eulerAngles.y, _downRotation.eulerAngles.z);
        }

        private void LeverActionDoor()
        {
            _isUp = !_isUp;
            StartCoroutine(RotateLever());
            door.IsLocked = false;
            door.MoveDoorLocal();
            door.IsLocked = true;
        }

        private void LeverActionSound()
        {
            _isUp = !_isUp;
            StartCoroutine(RotateLever());
            audioSource.Stop();
        }

        private void LeverAction()
        {
            _isUp = !_isUp;
            StartCoroutine(RotateLever());
            Debug.Log("Lever pressed!");
            _hasMoved = true;
        }

        public void Interact(Transform interactor)
        {
            InteractRpc();
            _hasMoved = true;
        }

        [Rpc(SendTo.Everyone)]
        private void InteractRpc()
        {
            if (!_isMoving)
            {
                if (audioSource is not null)
                {
                    LeverActionSound();
                }
                else
                {
                    LeverAction();
                }
            }
        }

        public bool CanInteract() => !_isMoving && !_hasMoved;

        private System.Collections.IEnumerator RotateLever()
        {
            _isMoving = true;
            Quaternion targetRotation = _isUp ? _upRotation : _downRotation;
            while (Quaternion.Angle(leverHandle.localRotation, targetRotation) > 0.1f)
            {
                leverHandle.localRotation = Quaternion.Lerp(leverHandle.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
                yield return null;
            }
            leverHandle.localRotation = targetRotation;
            _isMoving = false;
        }
    }

}