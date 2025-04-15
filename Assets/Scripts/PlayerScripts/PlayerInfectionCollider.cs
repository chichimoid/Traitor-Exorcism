using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerInfectionCollider : NetworkBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            var thisPlayerRole = NetworkPlayer.GetLocalInstance().Role;
            if (other.CompareTag("Player") && thisPlayerRole != PlayerRole.Monster && other.GetComponent<NetworkPlayer>().State == PlayerState.InMaze)
            {
                OnPlayerFound?.Invoke(other);
            }
        }

        void OnTriggerExit(Collider other)
        {
            var otherNetworkPlayer = other.GetComponent<NetworkPlayer>();
            if (other.CompareTag("Player") && otherNetworkPlayer.Role != PlayerRole.Monster && otherNetworkPlayer.State == PlayerState.InMaze)
            {
                OnPlayerLost?.Invoke(other);
            }
        }

        public delegate void OnPlayerFoundDelegate(Collider other);
        public event OnPlayerFoundDelegate OnPlayerFound;
        public delegate void OnPlayerLostDelegate(Collider other);
        public event OnPlayerLostDelegate OnPlayerLost;
    }
}