using Unity.Netcode;

namespace Maze.GameCycle
{
    public class Phase3Ender : NetworkBehaviour
    {
        private void Start()
        {
            
        }
        
        private void End()
        {
            OnPhase3Ended?.Invoke();
        }

        public delegate void OnPhase3EndedDelegate();
        public event OnPhase3EndedDelegate OnPhase3Ended;
    }
}