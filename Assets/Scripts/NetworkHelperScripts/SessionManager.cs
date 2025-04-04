using Unity.Netcode;
using UnityEngine;

namespace NetworkHelperScripts
{
    public class SessionManager : NetworkBehaviour
    {
        [SerializeField] private Transform gameManagerPrefab;
        
        public static SessionManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }
        
        public void LeaveSession()
        {
            if (IsServer)
            {
                KickEveryoneRpc();
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        public void KickPlayer(ulong playerId)
        {
            KickPlayerServerRpc(playerId);
        }

        [Rpc(SendTo.Server)]
        private void KickPlayerServerRpc(ulong playerId)
        {
            NetworkManager.Singleton.DisconnectClient(playerId);
        }
        
        [Rpc(SendTo.NotServer)]
        private void KickEveryoneRpc()
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}