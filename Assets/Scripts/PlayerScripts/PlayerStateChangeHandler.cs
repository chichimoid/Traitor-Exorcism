using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;
using Voting;

namespace PlayerScripts
{
    public class PlayerStateChangeHandler : NetworkBehaviour
    {
        private NetworkPlayer _networkPlayer;
        
        private void Start()
        {
            _networkPlayer = GetComponent<NetworkPlayer>();
            
            _networkPlayer.OnPlayerStateInVoting += HandleInVoting;
            _networkPlayer.OnPlayerStateFromInVoting += HandleFromInVoting;
        }
        
        private void HandleInVoting()
        {
            GetComponent<PlayerVoter>().enabled = true;

            PlayerLocker.Instance.LockMovement();
        }
        
        private void HandleFromInVoting()
        {
            GetComponent<PlayerVoter>().enabled = false;
            
            PlayerLocker.Instance.UnlockMovement();
        }
    }
}