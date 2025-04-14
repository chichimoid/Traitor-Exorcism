using System;
using System.Collections;
using Maze.GameCycle;
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
        
        private Phase1Initializer _phase1Initializer;
        private Phase1Ender _phase1Ender;
        private Phase2Initializer _phase2Initializer;
        private Phase2Ender _phase2Ender;
        private Phase3Initializer _phase3Initializer;
        private Phase3Ender _phase3Ender;
        private VotingPhaseInitializer _votingPhaseInitializer;
        private VotingPhaseEnder _votingPhaseEnder;

        private void Awake()
        {
            Instance = this;
        }

        public void StartGame()
        {
            StartMazeScene();
        }

        private void StartMazeScene()
        {
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Maze);
            ChangePlayerStatesRpc(PlayerState.InMaze);
        }
        
        public void OnMazeSceneStarted(Phase1Initializer p1Init, Phase1Ender p1End, Phase2Initializer p2Init, Phase2Ender p2End, Phase3Initializer p3Init, Phase3Ender p3End)
        {
            if (!IsServer) return;
            
            _phase1Initializer = p1Init;
            _phase1Ender = p1End;
            _phase2Initializer = p2Init;
            _phase2Ender = p2End;
            _phase3Initializer = p3Init;
            _phase3Ender = p3End;

            _phase1Ender.OnPhase1Ended += StartPhase2;
            _phase2Ender.OnPhase2Ended += StartPhase3;
            _phase3Ender.OnPhase3Ended += StartVotingScene;
            
            StartPhase1();
        }

        /// <summary>
        /// The peaceful phase for preparation.
        /// Roles are not distributed yet.
        /// </summary>
        private void StartPhase1()
        {
            if (!IsServer) return;
            
            if (Phase != GamePhase.None) throw new Exception($"Phase1 can only start after None. Active phase is {Phase}");
            Phase = GamePhase.Phase1;
            
            if (_phase1Initializer == null) throw new Exception("Phase initializer not found");
            Debug.Log("Starting phase 1...");
            _phase1Initializer.Init();
            
            _phase1Ender.Subscribe();
        }

        /// <summary>
        /// The suspicion phase.
        /// Roles are given, survivors are surviving, monster player is building up power.
        /// </summary>
        private void StartPhase2()
        {
            if (!IsServer) return;

            if (Phase != GamePhase.Phase1) throw new Exception($"Phase2 can only start after Phase1. Active phase is {Phase}");
            Phase = GamePhase.Phase2;
            
            if (_phase2Initializer == null) throw new Exception("Phase initializer not found");
            Debug.Log("Starting phase 2...");
            _phase2Initializer.Init();
            
            _phase2Ender.Subscribe(GetMonsterNetworkPlayer().GetComponent<MonsterBar>());
        }
        
        /// <summary>
        /// The reveal phase.
        /// Monster player turns to monster and tries to kill everyone.
        /// Survivors are trying to find all the levers and escape.
        /// </summary>
        private void StartPhase3()
        {
            if (!IsServer) return;

            if (Phase != GamePhase.Phase2) throw new Exception($"Phase3 can only start after Phase2. Active phase is {Phase}");
            Phase = GamePhase.Phase3;
            
            if (_phase3Initializer == null) throw new Exception("Phase initializer not found");
            Debug.Log("Starting phase 3...");
            _phase3Initializer.Init();
            
            _phase3Ender.Subscribe();
        }
        
        private void StartVotingScene()
        {
            ChangePlayerStatesRpc(PlayerState.InVoting);
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Voting);
        }
        
        public void OnVotingSceneStarted(VotingPhaseInitializer pVotingInit, VotingPhaseEnder pVotingEnd)
        {
            if (!IsServer) return;

            _votingPhaseInitializer = pVotingInit;
            _votingPhaseEnder = pVotingEnd;
            _votingPhaseEnder.OnVotingPhaseEnded += StartAftermath;
            
            StartVotingPhase();
        }
        
        /// <summary>
        /// The voting phase.
        /// All players vote for who they think is the monster.
        /// The chosen player gets kicked and players see if they were right.
        /// </summary>
        private void StartVotingPhase()
        {
            if (!IsServer) return;
            
            if (Phase != GamePhase.Phase3) throw new Exception($"Voting can only start after Phase3. Active phase is {Phase}");
            Phase = GamePhase.Voting;
            
            if (_votingPhaseInitializer == null) throw new Exception("Phase initializer not found");
            Debug.Log("Starting voting phase...");
            _votingPhaseInitializer.Init();
            
            _votingPhaseEnder.Subscribe();
        }
        
        /// <summary>
        /// The aftermath phase.
        /// Some game over scene etc.
        /// </summary>
        private void StartAftermath()
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
        
        public NetworkPlayer GetMonsterNetworkPlayer()
        {
            foreach (var (id, client) in NetworkManager.Singleton.ConnectedClients)
            {
                if (client.PlayerObject.GetComponent<NetworkPlayer>().Role == PlayerRole.Monster)
                {
                    return client.PlayerObject.GetComponent<NetworkPlayer>();
                }
            }
            throw new Exception("No monster found.");
        }
        
        [Rpc(SendTo.Everyone)]
        private void ChangePlayerStatesRpc(PlayerState state)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>().State = state;
        }
    }
}