using System.Collections.Generic;
using Maze;
using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace Voting
{
    public class VotingConcluder : NetworkBehaviour
    {
        public static VotingConcluder Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        
        public ulong GetPlayerToKickId()
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