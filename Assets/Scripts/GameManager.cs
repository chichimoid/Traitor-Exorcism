using System;
using System.Collections;
using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Voting;
using Random = UnityEngine.Random;

namespace Maze
{
    public enum GamePhase
    {
        None,
        Phase1,
        Phase2,
        Phase3,
        Voting,
        Conclusion,
    }
    
    public class GameManager : NetworkBehaviour
    {
        public GamePhase Phase { get; private set; } = GamePhase.None;
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        
        public void StartGame()
        {
            if (!IsServer) return;
            
            Debug.Log("Starting game...");
            
            StartPhase1();
        }

        /// <summary>
        /// The peaceful phase for preparation.
        /// Roles are not distributed yet.
        /// </summary>
        public void StartPhase1()
        {
            if (!IsServer) return;
            
            if (Phase != GamePhase.None) throw new Exception($"Phase1 can only start after None. Active phase is {Phase}");
            Phase = GamePhase.Phase1;
            
            Debug.Log("Starting phase 1...");
            
            ChangePlayerStatesRpc(PlayerState.InMaze);
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Maze);
        }

        /// <summary>
        /// The suspicion phase.
        /// Roles are given, survivors are surviving, monster player is building up power.
        /// </summary>
        public void StartPhase2()
        {
            if (!IsServer) return;

            if (Phase != GamePhase.Phase1) throw new Exception($"Phase2 can only start after Phase1. Active phase is {Phase}");
            Phase = GamePhase.Phase2;
            
            Debug.Log("Starting phase 2...");
            
            GiveRoles();
        }
        
        /// <summary>
        /// The reveal phase.
        /// Monster player turns to monster and tries to kill everyone.
        /// Survivors are trying to find all the levers and escape.
        /// </summary>
        public void StartPhase3()
        {
            if (!IsServer) return;

            if (Phase != GamePhase.Phase2) throw new Exception($"Phase3 can only start after Phase2. Active phase is {Phase}");
            Phase = GamePhase.Phase3;
            
            Debug.Log("Starting phase 3...");
            
            // TBA: Monster player turns to monster and stuff.
        }
        
        /// <summary>
        /// The voting phase.
        /// All players vote for who they think is the monster.
        /// The chosen player gets kicked and players see if they were right.
        /// </summary>
        public void StartVoting()
        {
            if (!IsServer) return;

            if (Phase != GamePhase.Phase3) throw new Exception($"Voting can only start after Phase3. Active phase is {Phase}");
            Phase = GamePhase.Voting;
            
            Debug.Log("Starting voting...");
            
            ChangePlayerStatesRpc(PlayerState.InVoting);
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Voting);
        }
        
        /// <summary>
        /// The aftermath phase.
        /// Some game over scene etc.
        /// </summary>
        public void StartAftermath()
        {
            if (!IsServer) return;
            
            if (Phase != GamePhase.Voting) throw new Exception($"Aftermath can only start after Voting. Active phase is {Phase}");
            
            Phase = GamePhase.Conclusion;
            
            Debug.Log("Starting aftermath...");
            
            // TBA: Some aftermath.
        }

        public ulong GetMonsterId()
        {
            foreach (var (id, client) in NetworkManager.Singleton.ConnectedClients)
            {
                if (client.PlayerObject.GetComponent<NetworkPlayer>().Role == PlayerRole.Monster)
                {
                    return id;
                }
            }
            throw new Exception("No monster found.");
        }
        
        private void GiveRoles()
        {
            if (!IsServer) return;
            
            var monsterIdIndex = Random.Range(0, NetworkManager.Singleton.ConnectedClientsList.Count);
            var monsterId = NetworkManager.Singleton.ConnectedClientsIds[monsterIdIndex];
            
            GiveRolesRpc(monsterId);
            
            GlobalDebugger.Instance.Log($"Player {monsterId} is the monster.");
        }
        
        [Rpc(SendTo.Everyone)]
        private void GiveRolesRpc(ulong monsterId)
        {
            var networkPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
            networkPlayer.Role = networkPlayer.Id == monsterId ? PlayerRole.Monster : PlayerRole.Survivor;
        }
        
        [Rpc(SendTo.Everyone)]
        private void ChangePlayerStatesRpc(PlayerState state)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>().State = state;
        }
    }
}