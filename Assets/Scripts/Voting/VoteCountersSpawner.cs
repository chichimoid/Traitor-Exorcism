using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Voting
{
    public class VoteCountersSpawner : NetworkBehaviour
    {
        [Header("Configure")] [SerializeField] private Vector3 voteCounterLocalSpawnOffset;
        
        [Header("References")]
        [SerializeField] private Transform voteCounterPrefab;
        public static VoteCountersSpawner Instance;

        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
            
            var position = transform.position + voteCounterLocalSpawnOffset;
            var rotation = transform.rotation;
            
            SpawnVoteCountTextServerRpc(position, rotation);

            
        }
        [ServerRpc(RequireOwnership = false)]
        private void SpawnVoteCountTextServerRpc(Vector3 position, Quaternion rotation)
        {
            Transform spawnedObject = Instantiate(voteCounterPrefab, position, rotation);
            NetworkObject spawnedNetworkObject = spawnedObject.GetComponent<NetworkObject>();
            spawnedNetworkObject.Spawn(true);
        }
    }
}