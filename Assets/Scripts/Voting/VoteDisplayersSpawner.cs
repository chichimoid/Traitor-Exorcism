using System.Linq;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Serialization;

namespace Voting
{
    public class VoteDisplayersSpawner : NetworkBehaviour
    {
        [Header("Configure")] [SerializeField] private Vector3 voteDisplayerLocalSpawnOffset;
        
        [Header("References")]
        [SerializeField] private Transform voteDisplayerPrefab;
        public static VoteDisplayersSpawner Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SpawnVoteCountTextServerRpc();
        }
        
        [Rpc(SendTo.Server)]
        private void SpawnVoteCountTextServerRpc()
        {
            foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var spawnedObject = Instantiate(voteDisplayerPrefab);
                var spawnedNetworkObject = spawnedObject.GetComponent<NetworkObject>();
                spawnedNetworkObject.Spawn(true);
                
                AssignCountersRpc(spawnedNetworkObject, RpcTarget.Single(id, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void AssignCountersRpc(NetworkObjectReference voteDisplayerReference, RpcParams rpcParams)
        {
            voteDisplayerReference.TryGet(out var voteDisplayerObject);
            
            var playerTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
            var position = playerTransform.position + voteDisplayerLocalSpawnOffset;
            var rotation = playerTransform.rotation;
            
            voteDisplayerObject.transform.position = Vector3.zero;
            voteDisplayerObject.transform.rotation = rotation;
            
            voteDisplayerObject.GetComponent<VoteDisplayer>().BoundId = NetworkManager.Singleton.LocalClientId;
        }
    }
}