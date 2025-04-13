using Unity.Netcode;

namespace Maze.GameCycle
{
    public class Phase3Initializer : NetworkBehaviour
    {
        public void Init()
        {
            GlobalDebugger.Instance.Log($"Phase 3 initialized.");
        }
    }
}