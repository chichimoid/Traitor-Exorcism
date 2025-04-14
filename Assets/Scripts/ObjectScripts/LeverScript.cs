using System;
using Maze;
using UnityEngine;
using Unity.Netcode;
using UnityEditorInternal;

namespace ObjectScripts
{
    public class LeverScript : NetworkBehaviour, IInteractable
    {
        [Header("Configure")]
        [SerializeField] private float rotationAngle = 90f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float yOffsetWhenHidden;
        [SerializeField] private float showMoveSpeed;

        [Header("References")] 
        [SerializeField] private Transform leverPillar;
        [SerializeField] private Transform leverHandle;
        [SerializeField] private Door door;

        private bool _isUp = false;
        private Quaternion _downRotation;
        private Quaternion _upRotation;
        private bool _isMoving = false;

        private void Start()
        {
            _downRotation = leverHandle.localRotation;
            _upRotation = Quaternion.Euler(_downRotation.eulerAngles.x - rotationAngle, _downRotation.eulerAngles.y, _downRotation.eulerAngles.z);
            
            LeverManager.Instance.OnShowLevers += Show;
            Hide();
        }
        
        private void LeverActionDoor()
        {
            _isUp = !_isUp;
            StartCoroutine(RotateLever());
            door.IsLocked = false;
            door.MoveDoorLocal();
            door.IsLocked = true;
        }
        
        private void LeverAction()
        {
            _isUp = !_isUp;
            StartCoroutine(RotateLever());
            Debug.Log("Lever pressed!");
        }

        public void Interact(Transform interactor)
        {
            IncrementLeverCountServerRpc();
            InteractRpc();
        }

        public void Hide()
        {
            leverPillar.transform.position -= new Vector3(0f, yOffsetWhenHidden, 0f);
        }

        public void Show()
        {
            StartCoroutine(ShowLeverCoroutine());
        }

        private System.Collections.IEnumerator ShowLeverCoroutine()
        {
            _isMoving = true;
            Vector3 targetPosition = leverPillar.transform.position + new Vector3(0f, yOffsetWhenHidden, 0f);
            while (Math.Abs((leverPillar.transform.position - targetPosition).y) > 0.01f)
            {
                leverPillar.transform.position = Vector3.Lerp(leverPillar.transform.position, targetPosition, Time.deltaTime * showMoveSpeed);
                yield return null;
            }
            _isMoving = false;
        }

        [Rpc(SendTo.Everyone)]
        private void InteractRpc()
        {
            if (!_isMoving)
            {
                if (door is not null)
                {
                    LeverActionDoor();
                }
                else
                {
                    LeverAction();
                }
            }
        }

        [Rpc(SendTo.Server)]
        private void IncrementLeverCountServerRpc()
        {
            LeverManager.Instance.IncrementLeverCount();
        }

        public bool CanInteract() => !_isMoving && GameManager.Instance.Phase == GamePhase.Phase3;

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