using ObjectScripts;
using UnityEngine;
using Unity.Netcode;

public class NetworkObjectSpawner : NetworkBehaviour
{
    [SerializeField] private SerializableSOList serializableSOList;
    public static NetworkObjectSpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnGrabbable(GrabbableSO grabbableSO, Vector3 position, Quaternion rotation)
    {
        SpawnGrabbableServerRpc(serializableSOList.grabbables.IndexOf(grabbableSO), position, rotation);
    }

    [Rpc(SendTo.Server)]
    private void SpawnGrabbableServerRpc(int SOIndex, Vector3 position, Quaternion rotation)
    {
        var grabbableSO = serializableSOList.grabbables[SOIndex];
        var spawnedObject = Instantiate(grabbableSO.prefab, position, rotation);
        var spawnedNetworkObject = spawnedObject.GetComponent<NetworkObject>();
        spawnedNetworkObject.Spawn(true);
    }
}