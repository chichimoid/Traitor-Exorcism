using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Maze.GameCycle
{
    public class Phase3Ender : NetworkBehaviour
    {
        [SerializeField] private LeverManager leverManager;
        public void Subscribe()
        {
            leverManager.OnAllLeversPulled += End;
        }
        
        private void End()
        {
            OnPhase3Ended?.Invoke();
        }

        public delegate void OnPhase3EndedDelegate();
        public event OnPhase3EndedDelegate OnPhase3Ended;
    }
}