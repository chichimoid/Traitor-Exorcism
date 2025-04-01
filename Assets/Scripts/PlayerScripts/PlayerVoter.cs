using ObjectScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Voting
{
    public class PlayerVoter : MonoBehaviour
    {
        [Header("Configure")]
        [SerializeField] private float votingRange = 12;
        
        private PlayerRayCaster _rayCaster;
        private NetworkPlayer _currentVotePlayer;
        private bool _someoneIsVoted = false;

        private void Start()
        {
            _rayCaster = GetComponent<PlayerRayCaster>();

            enabled = false;
        }

        private void Update()
        {
            TryVote();
        }
        
        private void TryVote()
        {
            if (_rayCaster.ViewPointRayCast(out RaycastHit hit, votingRange)) 
            {
                if (hit.collider.TryGetComponent(out NetworkPlayer otherPlayer) && otherPlayer.State == PlayerState.InVoting)
                {
                    if (_someoneIsVoted && _currentVotePlayer.Id == otherPlayer.Id) return;
                    if (_someoneIsVoted) VoteManager.Instance.UnVote(_currentVotePlayer.Id);
                    VoteManager.Instance.Vote(otherPlayer.Id);
                    _currentVotePlayer= otherPlayer;
                    _someoneIsVoted = true;
                }
                else if (_someoneIsVoted)
                {
                    VoteManager.Instance.UnVote(_currentVotePlayer.Id);
                    _currentVotePlayer = null;
                    _someoneIsVoted = false;
                }
            }
        }
    }
}
