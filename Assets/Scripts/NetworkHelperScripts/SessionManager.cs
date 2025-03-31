using Unity.Netcode;

namespace NetworkHelperScripts
{
    public class SessionManager : NetworkBehaviour
    {
        public static SessionManager Instance;
        private void Awake()
        {
            Instance = this;
            
            DontDestroyOnLoad(this.gameObject);
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