using ObjectScripts;
using UnityEngine;
using Unity.Netcode;
namespace ObjectScripts
{
    public class Door : NetworkBehaviour, IInteractable
    {
        [SerializeField] private Transform doorTransform; 
        [SerializeField] private float rotationAngle = 90f; 
        [SerializeField] private float rotationSpeed = 3f; 

        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private bool _isOpen = false;
        private bool _isMoving = false;
        public bool IsLocked = false;
        private void Start()
        {
            _closedRotation = doorTransform.localRotation;
            _openRotation = Quaternion.Euler(_closedRotation.eulerAngles.x, _closedRotation.eulerAngles.y + rotationAngle, _closedRotation.eulerAngles.z);
        }

        public void Interact(Transform interactor)
        {
            if (!_isMoving && !IsLocked)
            {
                _isOpen = !_isOpen;
                StartCoroutine(RotateDoor());
            }
        }

        public bool CanInteract() => !_isMoving;

        private System.Collections.IEnumerator RotateDoor()
        {
            _isMoving = true;
            Quaternion targetRotation = _isOpen ? _openRotation : _closedRotation;

            while (Quaternion.Angle(doorTransform.localRotation, targetRotation) > 0.1f)
            {
                doorTransform.localRotation = Quaternion.Lerp(doorTransform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
                yield return null;
            }

            doorTransform.localRotation = targetRotation;
            _isMoving = false;
        }
    }
}
