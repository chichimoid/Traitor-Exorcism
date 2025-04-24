using TMPro;
using UnityEngine;

namespace PlayerScripts.UI
{
    public class LoadingScreenUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject uiObject;

        private string _roleDependentVictoryText;
        private string _roleDependentLossText;
        
        private void Start()
        {
            uiObject.SetActive(false);
        }
        
        public void Show()
        {
            uiObject.SetActive(true);
        }

        public void Hide()
        {
            uiObject.SetActive(false);
        }
    }
}