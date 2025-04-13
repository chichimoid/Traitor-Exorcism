using PlayerScripts;
using Unity.Netcode;

namespace Maze.GameCycle
{
    public class Phase2Ender : NetworkBehaviour
    {
        public void Subscribe(MonsterBar monsterBar)
        {
            monsterBar.OnMonsterTurned += End;
        }
        
        private void End()
        {
            OnPhase2Ended?.Invoke();
        }

        public delegate void OnPhase2EndedDelegate();
        public event OnPhase2EndedDelegate OnPhase2Ended;
    }
}