using UnityEngine;

namespace PlayerScripts
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private GameObject interactionUI;
        [SerializeField] private GameObject emoteWheelUI;
        [SerializeField] private GameObject pauseMenuUI;
    
        public GameObject InteractionUI => interactionUI;
        public GameObject EmoteWheelUI => emoteWheelUI;
        public GameObject PauseMenuUI => pauseMenuUI;
    
        public static PlayerUI Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    
        /// <summary>
        /// To be called on player spawn.
        /// Disables all UI that is automatically shown under certain conditions (e.g. interaction UI).
        /// </summary>
        public void DisableConditionalUI()
        {
            interactionUI.SetActive(false);
        }
    }
}
