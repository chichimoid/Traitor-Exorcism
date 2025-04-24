using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerInfectionCollider : NetworkBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            var thisPlayer = NetworkPlayer.GetLocalInstance();
            if (other.CompareTag("Player") && thisPlayer.Role != PlayerRole.Monster && thisPlayer.State == PlayerState.InMaze)
            {
                OnPlayerFound?.Invoke(other);
            }
        }

        void OnTriggerExit(Collider other)
        {
            var thisPlayer = NetworkPlayer.GetLocalInstance();
            if (other.CompareTag("Player") && thisPlayer.Role != PlayerRole.Monster && thisPlayer.State == PlayerState.InMaze)
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