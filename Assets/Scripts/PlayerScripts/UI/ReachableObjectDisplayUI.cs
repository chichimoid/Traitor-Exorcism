using UnityEngine;

namespace PlayerScripts.UI
{
    public class ReachableObjectDisplayUI : MonoBehaviour
    {
        [SerializeField] private GameObject interactionUIObject;
        
        private ReachableObjectDetector _reachableObjectDetector;
        
        private void Start()
        {
            _reachableObjectDetector = NetworkPlayer.GetLocalInstance().GetComponent<ReachableObjectDetector>();

            interactionUIObject.SetActive(false);
            
            OnEnable();
        }

        private void OnEnable()
        {
            if (!didStart) return;
            
            _reachableObjectDetector.OnInteractableFound += Show;
            _reachableObjectDetector.OnInteractableLost += Hide;
        }

        private void OnDisable()
        {
            _reachableObjectDetector.OnInteractableFound -= Show;
            _reachableObjectDetector.OnInteractableLost -= Hide;
        }

        public void Show()
        {
            interactionUIObject.SetActive(true);
        }
        
        public void Hide()
        {
            interactionUIObject.SetActive(false);
        }
    }
}