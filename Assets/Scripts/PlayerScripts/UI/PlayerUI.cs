using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerScripts.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private ReachableObjectDisplayUI reachableObjectDisplayUI;
        [SerializeField] private ActivatableMenu emoteWheelUI;
        [SerializeField] private ActivatableMenu pauseMenuUI;
        [SerializeField] private RoleDisplayUI roleDisplayUI;
        
        public ReachableObjectDisplayUI ReachableObjectDisplayUI => reachableObjectDisplayUI;
        public ActivatableMenu EmoteWheelUI => emoteWheelUI;
        public ActivatableMenu PauseMenuUI => pauseMenuUI;
        public RoleDisplayUI RoleDisplayUI => roleDisplayUI;
        public static PlayerUI Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }
    }
}
