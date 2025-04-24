using System;
using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace Voting.GameCycle
{
    public class VotingPhaseInitializer : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private int votingTimeSeconds;
        
        [Header("References")]
        [SerializeField] private ServerTimer serverTimer;

        public void Init()
        {
            serverTimer.StartTimer(votingTimeSeconds);
            
            GlobalDebugger.Instance.Log("Voting phase initialized");
        }
    }
}