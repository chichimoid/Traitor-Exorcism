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
                KickEveryoneServerRpc();
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

        [ServerRpc]
        private void KickPlayerServerRpc(ulong playerId)
        {
            NetworkManager.Singleton.DisconnectClient(playerId);
        }
        
        [ServerRpc]
        private void KickEveryoneServerRpc()
        {
            KickEveryoneClientRpc();
        }
        [ClientRpc]
        private void KickEveryoneClientRpc()
        {
            if (IsServer) return;
            NetworkManager.Singleton.Shutdown();
        }
    }
}