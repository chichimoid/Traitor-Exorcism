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
            HandleInVotingServerRpc(NetworkManager.Singleton.LocalClient.PlayerObject);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HandleInVotingServerRpc(NetworkObjectReference playerReference)
        {
            playerReference.TryGet(out var playerObject);

            playerObject.GetComponent<PlayerVoter>().enabled = true;

            playerObject.GetComponent<PlayerLocker>().LockMovement();
        }
        
        private void HandleFromInVoting()
        {
            HandleFromInVotingServerRpc(gameObject);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HandleFromInVotingServerRpc(NetworkObjectReference playerReference)
        {
            playerReference.TryGet(out var playerObject);
            
            playerObject.GetComponent<PlayerVoter>().enabled = false;
            
            playerObject.GetComponent<PlayerLocker>().UnlockMovement();
        }
    }
}