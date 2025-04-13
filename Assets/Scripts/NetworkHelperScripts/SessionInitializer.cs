using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NetworkHelperScripts
{
    public class SessionInitializer : NetworkBehaviour
    {
        [SerializeField] private List<Transform> intersceneObjectsPrefabs = new();
        
        public override void OnNetworkSpawn()
        {
            foreach (var prefab in intersceneObjectsPrefabs)
            {
                var spawnedObject = Instantiate(prefab);
                var spawnedNetworkObject = spawnedObject.GetComponent<NetworkObject>();
                spawnedNetworkObject.Spawn(false);
            }
            
            Destroy(gameObject);
        }
    }
}