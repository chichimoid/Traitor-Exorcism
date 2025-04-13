using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;

namespace Maze.GameCycle
{
    public class Phase1Ender : NetworkBehaviour
    {
        [SerializeField] private ServerTimer serverTimer;

        private void Start()
        {
            serverTimer.OnTimeUp += End;
        }
        
        private void End()
        {
            OnPhase1Ended?.Invoke();
        }

        public delegate void OnPhase1EndedDelegate();
        public event OnPhase1EndedDelegate OnPhase1Ended;
    }
}