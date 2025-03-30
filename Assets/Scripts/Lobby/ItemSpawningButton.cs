using ObjectScripts;
using Unity.Netcode;
using UnityEngine;

namespace Lobby
{
    public class ItemSpawningButton : NetworkBehaviour, IInteractable
    {
        [SerializeField] private GrabbableSO grabbable;

        public void Interact(Transform player)
        {
            NetworkObjectSpawner.Instance.SpawnGrabbable(grabbable, transform.position + 2 * Vector3.up + Random.insideUnitSphere, Random.rotation);
        }
    }
}