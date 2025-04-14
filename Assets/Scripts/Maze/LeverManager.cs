using ObjectScripts;
using Unity.Netcode;

namespace Maze
{
    public class LeverManager : NetworkBehaviour
    {
        private int _leversToPull = 5; // TBA: make dependent on player count.
        private int _leverCount = 0;
        
        public static LeverManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void ShowLevers()
        {
            ShowLeversRpc();
        }
        
        [Rpc(SendTo.Everyone)]
        private void ShowLeversRpc() {
            OnShowLevers?.Invoke();
        }
        
        public void IncrementLeverCount()
        {
            if (++_leverCount >= _leversToPull)
            {
                OnAllLeversPulled?.Invoke();
            }
        }
        
        public delegate void OnAllLeversPulledDelegate();
        public event OnAllLeversPulledDelegate OnAllLeversPulled;
        
        public delegate void OnShowLeversDelegate();
        public event OnShowLeversDelegate OnShowLevers;
    }
}