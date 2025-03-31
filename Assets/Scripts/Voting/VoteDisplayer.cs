using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Voting
{
    public class VoteDisplayer : NetworkBehaviour
    {
        [SerializeField] private TMP_Text textField;
        public ulong BoundId { get; set; }

        public int Votes { get; private set; } = 0;

        private void Start()
        {
            textField.text = Votes.ToString();

            VoteManager.Instance.OnVote += Vote;
            VoteManager.Instance.OnUnVote += UnVote;
        }

        public void Vote(ulong id)
        {
            if (BoundId == id)
            {
                VoteRpc(gameObject);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void VoteRpc(NetworkObjectReference voteCounterReference)
        {
            voteCounterReference.TryGet(out var voteCounterObject);
            voteCounterObject.GetComponent<VoteDisplayer>().textField.text = (++Votes).ToString();
        }

        public void UnVote(ulong id)
        {
            if (BoundId == id)
            {
                VoteRpc(gameObject);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void UnVoteRpc(NetworkObjectReference voteCounterReference)
        {
            voteCounterReference.TryGet(out var voteCounterObject);
            voteCounterObject.GetComponent<VoteDisplayer>().textField.text = (--Votes).ToString();
        }
    }
}
