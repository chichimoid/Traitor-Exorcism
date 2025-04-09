using System.Collections.Generic;
using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;
using Voting;

namespace Maze
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
            GameManager.Instance.StartPhase2();
        }
    }
}