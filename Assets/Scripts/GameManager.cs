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
        [Header("Voting Configure")] 
        [SerializeField] private Vector2 votingCircleCenter;
        [SerializeField] private float votingCircleRadius;
        
        public static GameManager Instance;

        private void Awake()
        {
            Instance = this;
            
            DontDestroyOnLoad(this.gameObject);
        }

        private void ChangeStates(PlayerState state)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                client.PlayerObject.GetComponent<NetworkPlayer>().State = state;
            }
        }
        
        private void GiveRoles()
        {
            if (!IsServer) return;
            
            var monsterIdIndex = Random.Range(0, NetworkManager.Singleton.ConnectedClientsList.Count);
            var monsterId = NetworkManager.Singleton.ConnectedClientsIds[monsterIdIndex];

            foreach (var (clientId, client) in NetworkManager.Singleton.ConnectedClients)
            {
                if (clientId == monsterId)
                {
                    var networkPlayer = client.PlayerObject.GetComponent<NetworkPlayer>();
                    networkPlayer.Role = PlayerRole.Monster;
                    
                    GlobalDebugger.Instance.Log($"Player {clientId} is the monster.");
                }
            }
        }
        
        public void StartGame()
        {
            if (!IsServer) return;
            
            Debug.Log("Starting game...");
            
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Maze);
            
            ChangeStates(PlayerState.InMaze);
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
            StartVotingServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void StartVotingServerRpc()
        {
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Voting);
            
            ChangeStates(PlayerState.InVoting);
            
            var len = NetworkManager.Singleton.ConnectedClients.Count;
            for (int i = 0; i < len; i++)
            {
                var playerTransform = NetworkManager.Singleton.ConnectedClientsList[i].PlayerObject.gameObject.transform;
                playerTransform.position = new Vector3(
                    votingCircleCenter.x + (float)Math.Sin(Math.PI * 2 * i / len) * votingCircleRadius,
                    playerTransform.position.y,
                    votingCircleCenter.y + (float)Math.Cos(Math.PI * 2 * i / len) * votingCircleRadius);
                playerTransform.LookAt(new Vector3(votingCircleCenter.x, playerTransform.position.y, votingCircleCenter.y));
            }
        }

        private void EndVoting()
        {
            if (!IsServer) return;

            NetworkPlayer maxVotesPlayer = null;
            int maxVotes = 0;
            
            foreach (var (clientId, client) in NetworkManager.Singleton.ConnectedClients)
            {
                var networkPlayer = client.PlayerObject.GetComponent<NetworkPlayer>();

                var votes = networkPlayer.GetComponent<VoteCount>().Votes;
                
                if (votes > maxVotes)
                {
                    maxVotes = votes;
                    maxVotesPlayer = networkPlayer;
                }
                else if (votes == maxVotes)
                {
                    maxVotesPlayer = null;
                }
            }
            
            if (maxVotesPlayer != null) Kick(maxVotesPlayer);
        }

        public void Kick(NetworkPlayer player)
        {
            if (!IsServer) return;
            
            SessionManager.Instance.KickPlayer(player.Id);
        }
    }
}