using System;
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
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
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
        private void ChangeStatesRpc(PlayerState state)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>().State = state;
        }
        
        public void StartGame()
        {
            if (!IsServer) return;
            
            Debug.Log("Starting game...");
            
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Maze);
            
            ChangeStatesRpc(PlayerState.InMaze);
            GiveRoles();
            StartRound();
        }

        public void StartRound()
        {
            if (!IsServer) return;
            
            // TBA
        }
        
        public void StartVoting()
        {
            ChangeStatesRpc(PlayerState.InVoting);
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Voting);
        }
        
        private void EndVoting()
        {
            if (!IsServer) return;

            int maxVotes = 0;
            bool doKickPlayer = false;
            foreach (var voteCount in VoteManager.Instance.Votes)
            {
                if (voteCount > maxVotes)
                {
                    maxVotes = voteCount;
                    doKickPlayer = true;
                }
                else if (voteCount == maxVotes)
                {
                    doKickPlayer = false;
                }
            }

            if (doKickPlayer)
            {
                var index = VoteManager.Instance.Votes.IndexOf(maxVotes);
                var maxVotesPlayerId = NetworkManager.Singleton.ConnectedClientsIds[index];
                SessionManager.Instance.KickPlayer(maxVotesPlayerId);
            }
        }
    }
}