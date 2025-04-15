using UnityEngine;

namespace PlayerScripts.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private ReachableObjectDisplayUI reachableObjectDisplayUI;
        [SerializeField] private ActivatableMenu emoteWheelUI;
        [SerializeField] private ActivatableMenu pauseMenuUI;
        [SerializeField] private RoleDisplayUI roleDisplayUI;
        [SerializeField] private EndScreenUI endScreenUI;
        
        public ReachableObjectDisplayUI ReachableObjectDisplayUI => reachableObjectDisplayUI;
        public ActivatableMenu EmoteWheelUI => emoteWheelUI;
        public ActivatableMenu PauseMenuUI => pauseMenuUI;
        public RoleDisplayUI RoleDisplayUI => roleDisplayUI;
        public EndScreenUI EndScreenUI => endScreenUI;
        public static PlayerUI Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }
    }
}
