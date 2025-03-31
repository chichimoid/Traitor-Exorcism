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
        private NetworkPlayer _currentVotePlayer = null;

        private void Start()
        {
            _rayCaster = GetComponent<PlayerRayCaster>();

            enabled = false;
        }

        private void Update()
        {
            Vote();
        }
        
        private void Vote()
        {
            if (_rayCaster.ViewPointRayCast(out RaycastHit hit, votingRange)) 
            {
                if (hit.collider.TryGetComponent(out NetworkPlayer otherPlayer) && otherPlayer.State == PlayerState.InVoting)
                {
                    if (_currentVotePlayer != null && _currentVotePlayer.Id == otherPlayer.Id) return;
                    //Debug.Log("pidorasi");
                    VoteManager.Instance.UnVote(_currentVotePlayer.Id);
                    _currentVotePlayer= otherPlayer;
                    VoteManager.Instance.Vote(otherPlayer.Id);
                }
                else
                {
                    _currentVotePlayer = null;
                }
            }
        }
    }
}
