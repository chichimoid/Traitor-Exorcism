using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Voting;

namespace Maze
{
    public class Phase1Initializer : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private int minPhaseDurationSeconds;
        [SerializeField] private int maxPhaseDurationSeconds;
        
        [Header("References")] 
        [SerializeField] private ServerTimer serverTimer;
        
        public void StartPhase1()
        {
            int phaseDuration = Random.Range(minPhaseDurationSeconds, maxPhaseDurationSeconds);
            
            Debug.Log($"Starting phase 1 at {phaseDuration} seconds");
            
            serverTimer.StartTimer(phaseDuration);
        }
    }
}