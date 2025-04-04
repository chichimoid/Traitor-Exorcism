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

        private bool _isUp = false;
        private Quaternion _downRotation;
        private Quaternion _upRotation;
        private bool _isMoving = false;

        private void Start()
        {
            _downRotation = leverHandle.localRotation;
            _upRotation = Quaternion.Euler(_downRotation.eulerAngles.x - rotationAngle, _downRotation.eulerAngles.y, _downRotation.eulerAngles.z);
        }

        private void LeverActionDoor(Transform interactor)
        {
            _isUp = !_isUp;
            StartCoroutine(RotateLever());
            door.IsLocked = false;
            door.Interact(interactor);
            door.IsLocked = true;

        }
        private void LeverAction(Transform interactor)
        {
            _isUp = !_isUp;
            StartCoroutine(RotateLever());
            Debug.Log("Lever pressed!");
        }

        public void Interact(Transform interactor)
        {
            if (!_isMoving)
            {
                if (door is not null)
                {
                    LeverActionDoor(interactor);
                }
                else
                {
                    LeverAction(interactor);
                }
            }
        }

        public bool CanInteract() => !_isMoving;

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