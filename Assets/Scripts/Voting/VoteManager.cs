using System.Collections.Generic;
using System.Linq;
using PlayerScripts;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Voting
{
    public class VoteManager : NetworkBehaviour
    {
        public NetworkList<int> Votes = new();
        public static VoteManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Vote(ulong id)
        {
            VoteServerRpc(id);
        }

        [Rpc(SendTo.Server)]
        private void VoteServerRpc(ulong id)
        {
            ++Votes[NetworkManager.Singleton.ConnectedClientsIds.ToList().IndexOf(id)];
            
            VoteRpc(id);
        }

        [Rpc(SendTo.Everyone)]
        private void VoteRpc(ulong id)
        {
            OnVote?.Invoke(id);
        }

        public void UnVote(ulong id)
        {
            UnVoteServerRpc(id);
        }

        [Rpc(SendTo.Server)]
        private void UnVoteServerRpc(ulong id)
        {
            --Votes[NetworkManager.Singleton.ConnectedClientsIds.ToList().IndexOf(id)];
            
            UnVoteRpc(id);
        }

        [Rpc(SendTo.Everyone)]
        private void UnVoteRpc(ulong id)
        {
            OnUnVote?.Invoke(id);
        }
        
        public delegate void VoteDelegate(ulong id);
        public event VoteDelegate OnVote;
        public event VoteDelegate OnUnVote;
    }
}