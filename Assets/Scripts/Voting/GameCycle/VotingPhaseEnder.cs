﻿using System.Collections.Generic;
using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;

namespace Voting.GameCycle
{
    public class VotingPhaseEnder : NetworkBehaviour
    {
        [SerializeField] private ServerTimer serverTimer;

        public void Subscribe()
        {
            serverTimer.OnTimeUp += End;
        }
        
        private void End()
        {
            var playerToKickId = GetPlayerToKickId();
            if (playerToKickId == GameManager.GetMonsterId())
            {
                GlobalDebugger.Instance.Log($"Player {playerToKickId} was indeed the monster.");
            }
            else
            {
                GlobalDebugger.Instance.Log($"Player {playerToKickId} was not the monster.");
            }
            
            OnVotingPhaseEnded?.Invoke(playerToKickId);
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
                    maxVotesPlayersIds.Add(GameManager.Instance.AlivePlayersIds[i]);
                    maxVotes = voteCount;
                }
                else if (voteCount == maxVotes)
                {
                    maxVotesPlayersIds.Add(GameManager.Instance.AlivePlayersIds[i]);
                }
            }

            return maxVotesPlayersIds;
        }
        
        public delegate void OnVotingPhaseEndedDelegate(ulong votedPlayerId);
        public event OnVotingPhaseEndedDelegate OnVotingPhaseEnded;
    }
}