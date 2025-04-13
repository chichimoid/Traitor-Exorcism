using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;

namespace Maze.GameCycle
{
    public class Phase1Initializer : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private int minPhaseDurationSeconds;
        [SerializeField] private int maxPhaseDurationSeconds;
        
        [Header("References")] 
        [SerializeField] private ServerTimer serverTimer;
        
        public void Init()
        {
            int phaseDuration = Random.Range(minPhaseDurationSeconds, maxPhaseDurationSeconds);
            
            serverTimer.StartTimer(phaseDuration);
            
            GlobalDebugger.Instance.Log($"Phase 1 initialized at {phaseDuration} seconds");
        }
    }
}