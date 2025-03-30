using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Voting
{
    public class VoteCount : NetworkBehaviour
    {
        [SerializeField] private TMP_Text textField;

        public int Votes { get; private set; }

        public void VoteThis()
        {
            VoteThisServerRpc();
        }

        public void UnVoteThis()
        {
            UnVoteThisServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void VoteThisServerRpc()
        {
            VoteThisClientRpc();
        }

        [ClientRpc]
        private void VoteThisClientRpc()
        {
            textField.text = (++Votes).ToString();
        }

        [ServerRpc(RequireOwnership = false)]
        private void UnVoteThisServerRpc()
        {
            UnVoteThisClientRpc();
        }

        [ClientRpc]
        private void UnVoteThisClientRpc()
        {
            textField.text = (--Votes).ToString();
        }
    }
}
