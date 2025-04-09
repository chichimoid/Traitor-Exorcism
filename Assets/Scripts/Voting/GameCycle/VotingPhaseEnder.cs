using System.Collections.Generic;
using Maze;
using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Voting
{
    public class VotingPhaseEnder : NetworkBehaviour
    {
        [SerializeField] private ServerTimer serverTimer;

        private void Start()
        {
            serverTimer.OnTimeUp += End;
        }
        
        private void End()
        {
            var playerToKickId = GetPlayerToKickId();
            if (playerToKickId == GameManager.Instance.GetMonsterId())
            {
                GlobalDebugger.Instance.Log($"Player {playerToKickId} was indeed the monster.");
            }
            else
            {
                GlobalDebugger.Instance.Log($"Player {playerToKickId} was not the monster.");
            }
            
            GameManager.Instance.StartAftermath();
        }
        
        private ulong GetPlayerToKickId()
        {
            var maxVotesPlayersIds = GetMaxVotesPlayersIds();
            var playerToKickIdIndex = Random.Range(0, maxVotesPlayersIds.Count);
            var playerToKickId = maxVotesPlayersIds[playerToKickIdIndex];
            return playerToKickId;
        }

        private List<ulong> GetMaxVotesPlayersIds()
        {
            List<ulong> maxVotesPlayersIds = new();
            int maxVotes = 0;
            for (int i = 0; i < VoteManager.Instance.Votes.Count; i++)
            {
                var voteCount = VoteManager.Instance.Votes[i];
                if (voteCount > maxVotes)
                {
                    maxVotesPlayersIds.Clear();
                    maxVotesPlayersIds.Add(NetworkManager.Singleton.ConnectedClientsIds[i]);
                    maxVotes = voteCount;
                }
                else if (voteCount == maxVotes)
                {
                    maxVotesPlayersIds.Add(NetworkManager.Singleton.ConnectedClientsIds[i]);
                }
            }

            return maxVotesPlayersIds;
        }
    }
}