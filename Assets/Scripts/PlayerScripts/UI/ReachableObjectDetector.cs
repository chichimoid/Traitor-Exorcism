using ObjectScripts;
using UnityEngine;

namespace PlayerScripts.UI
{
    /// <summary>
    /// Shows UI for interaction, attack etc. TBA: extend for differentiation
    /// </summary>
    public class ReachableObjectDetector : MonoBehaviour
    {
        private float _interactionRange;
        private PlayerRayCaster _rayCaster;
        private bool _interactablePresent = false;
        
        private void Start()
        {
            _interactionRange = GetComponent<PlayerInteract>().InteractionRange;
            
            _rayCaster = GetComponent<PlayerRayCaster>();
        }
        private void Update()
        {
            UpdateCrosshairUI();
        }

        private void UpdateCrosshairUI()
        {
            bool interactablePresent = false;
            if (_rayCaster.ViewPointRayCast(out RaycastHit hit, _interactionRange))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
                {
                    interactablePresent = true;
                }
            }

            if (interactablePresent && !_interactablePresent)
            {
                _interactablePresent = true;
                OnInteractableFound?.Invoke();
            }
            else if (!interactablePresent && _interactablePresent)
            {
                _interactablePresent = false;
                OnInteractableLost?.Invoke();
            }
        }

        public delegate void OnInteractableFoundDelegate();
        public event OnInteractableFoundDelegate OnInteractableFound;
        public delegate void OnInteractableLostDelegate();
        public event OnInteractableLostDelegate OnInteractableLost;
    }
}