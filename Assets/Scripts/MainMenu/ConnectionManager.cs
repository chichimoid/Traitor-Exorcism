using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace MainMenu
{
    public class ConnectionManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] int maxConnections;
        public string LobbyCode { get; private set; }
        public static ConnectionManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        
            DontDestroyOnLoad(gameObject);
        }

        public async void CreateRelay()
        {
            var hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            LobbyCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            // Changed according to Unity documentation due to deprecation of Relay package
            // var relayServerData = new RelayServerData(allocation, "dtls");
            var relayServerData = AllocationUtils.ToRelayServerData(hostAllocation, "dtls");
        
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        
            NetworkManager.Singleton.StartHost();
        }

        public async void JoinRelay(string joinCode)
        {
            var clientAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        
            // Changed according to Unity documentation due to deprecation of Relay package
            // var relayServerData = new RelayServerData(allocation, "dtls");
            var relayServerData = AllocationUtils.ToRelayServerData(clientAllocation, "dtls");
        
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            NetworkManager.Singleton.StartClient();
        }
    }
}