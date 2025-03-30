using ObjectScripts;
using UnityEngine;

namespace PlayerScripts
{
    /// <summary>
    /// Shows a universal UI for interacton/attack. TBA: extend for differentiation
    /// </summary>
    public class CrosshairUIManager : MonoBehaviour
    {
        private float _interactionRange;
        
        private PlayerRayCaster _rayCaster;
        private GameObject _interactionUI;

        private void Start()
        {
            _interactionRange = GetComponent<PlayerInteract>().InteractionRange;
            
            _rayCaster = GetComponent<PlayerRayCaster>();
            _interactionUI = PlayerUI.Instance.InteractionUI;
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

            _interactionUI.SetActive(interactablePresent);
        }
    }
}