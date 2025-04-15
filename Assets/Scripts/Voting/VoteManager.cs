using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Voting
{
    public class VoteManager : NetworkBehaviour
    {
        public readonly NetworkList<int> Votes = new(new List<int>());
        public static VoteManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (!IsServer) return;
            
            for (int i = 0; i < GameManager.Instance.AlivePlayersIds.Count; i++)
            {
                Votes.Add(0);
            }
        }

        public void Vote(ulong id)
        {
            VoteServerRpc(id);
        }

        [Rpc(SendTo.Server)]
        private void VoteServerRpc(ulong id)
        {
            ++Votes[GameManager.Instance.AlivePlayersIds.IndexOf(id)];
            
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
            --Votes[GameManager.Instance.AlivePlayersIds.IndexOf(id)];
            
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