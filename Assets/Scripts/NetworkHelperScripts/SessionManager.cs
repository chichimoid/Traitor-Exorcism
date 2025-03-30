using Unity.Netcode;
using UnityEngine;

namespace NetworkHelperScripts
{
    public class SessionManager : NetworkBehaviour
    {
        [SerializeField] private Transform gameManagerPrefab;
        
        public static SessionManager Instance;
        private void Awake()
        {
            Instance = this;
            
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            if (!IsServer)
            {
                Debug.Log("it worked");

                Transform spawnedObject = Instantiate(gameManagerPrefab);
                NetworkObject spawnedNetworkObject = spawnedObject.GetComponent<NetworkObject>();
                spawnedNetworkObject.Spawn(false);
            }
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