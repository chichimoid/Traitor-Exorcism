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
        [SerializeField] private float votingRange;
        
        private PlayerRayCaster _rayCaster;
        private NetworkPlayer _currentVotePlayer;

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
                    if (_currentVotePlayer.Id == otherPlayer.Id) return;
                    
                    _currentVotePlayer.GetComponent<VoteCount>().UnVoteThis();
                    _currentVotePlayer= otherPlayer;
                    otherPlayer.GetComponent<VoteCount>().VoteThis();
                }
            }
        }
    }
}
