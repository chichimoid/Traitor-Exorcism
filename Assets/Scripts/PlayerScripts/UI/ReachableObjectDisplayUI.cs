using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerScripts.UI
{
    public class ReachableObjectDisplayUI : MonoBehaviour
    {
        [SerializeField] private GameObject interactionUIObject;
        
        private ReachableObjectDetector _reachableObjectDetector;
        
        private void Start()
        {
            _reachableObjectDetector = NetworkPlayer.GetLocalInstance().GetComponent<ReachableObjectDetector>();
            _reachableObjectDetector.OnInteractableFound += Show;
            _reachableObjectDetector.OnInteractableLost += Hide;

            interactionUIObject.SetActive(false);
        }

        private void Show()
        {
            interactionUIObject.SetActive(true);
        }
        
        private void Hide()
        {
            interactionUIObject.SetActive(false);
        }
    }
}