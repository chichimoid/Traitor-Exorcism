using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Voting
{
    public class VoteDisplayer : NetworkBehaviour
    {
        [SerializeField] private TMP_Text textField;
        
        private readonly NetworkVariable<ulong> _boundId = new();
        
        public ulong BoundId { get => _boundId.Value; set => _boundId.Value = value; }
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
                textField.text = (++Votes).ToString();
            }
        }

        public void UnVote(ulong id)
        {
            if (BoundId == id)
            {
                textField.text = (--Votes).ToString();
            }
        }
    }
}
