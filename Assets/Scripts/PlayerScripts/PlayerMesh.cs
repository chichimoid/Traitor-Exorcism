using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayerScripts
{
    using Unity.Netcode;
    using UnityEngine;

    /// <summary>
    /// Only changes colors so far. TBA: different player meshes.
    /// </summary>
    public class PlayerMesh : NetworkBehaviour
    {
        [SerializeField] private List<Material> materials;
        [SerializeField] private Material monsterMaterial;
        private readonly List<int> _excludedMaterials = new();

        private readonly NetworkVariable<int> _playerMaterialIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private int PlayerMaterialIndex 
        { 
            get => _playerMaterialIndex.Value; 
            set => _playerMaterialIndex.Value = value;
        }
            
        [field:SerializeField] public MeshRenderer PlayerMeshRenderer { get; private set; }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (!IsOwner) return;
            
            if (_excludedMaterials.Count >= materials.Count)
            {
                throw new Exception("Too many players, need more materials!");
            }

            var availableIndices = Enumerable.Range(0, materials.Count).ToList();
            foreach (var excludedMaterial in _excludedMaterials)
            {
                availableIndices.Remove(excludedMaterial);
            }
            int availableIndexIndex = Random.Range(0, availableIndices.Count);
            int index = availableIndices[availableIndexIndex];

            SetOtherPlayersMaterials();
            
            // That coroutine is extremely suspicious, but hear me out.
            // In every piece of information I have seen, it is stated that you can use Rpc in OnNetworkSpawn().
            // But here, the Rpc just does not arrive to the client, since some spawning procedures are still running.
            // This is absolutely bogus, since OnNetworkSpawn() should literally run after the object has been fully spawned.
            // This might be due to the script being located on the player object itself, rather that somewhere else in the scene.
            // The only real fix for this goofy Netcode race condition is just waiting.
            // This definitely requires some more reliable fixing, but I just do not have the time right now.
            
            PlayerMaterialIndex = index;
            
            PlayerMeshRenderer.material = materials[index];
            _excludedMaterials.Add(index);

            SetPlayerMaterialNotMeRpc(gameObject, PlayerMaterialIndex);
        }

        private void SetOtherPlayersMaterials()
        {
            foreach (var (id, client) in NetworkManager.Singleton.ConnectedClients)
            {
                if (id == NetworkManager.Singleton.LocalClientId) return;           
                
                var otherPlayerMesh = client.PlayerObject.GetComponent<PlayerMesh>();
                otherPlayerMesh.PlayerMeshRenderer.material = materials[otherPlayerMesh.PlayerMaterialIndex];
            }
        }
    
        private IEnumerator WaitBecauseNetcode(int index)
        {
            yield return new WaitForSeconds(1);
            
            SetPlayerMaterialNotMeRpc(NetworkPlayer.GetLocalInstance().gameObject, index);
        }

        [Rpc(SendTo.NotMe)]
        private void SetPlayerMaterialNotMeRpc(NetworkObjectReference playerReference, int index)
        {
            playerReference.TryGet(out var playerNetworkObject);
            var playerMesh = playerNetworkObject.GetComponent<PlayerMesh>();
            playerMesh._excludedMaterials.Add(index);
            playerMesh.PlayerMeshRenderer.material = materials[index];
        }

        public void SetMonsterMaterial()
        {
            SetMonsterMaterialRpc(NetworkPlayer.GetLocalInstance().gameObject);
        }
        
        [Rpc(SendTo.Everyone)]
        private void SetMonsterMaterialRpc(NetworkObjectReference playerReference)
        {
            playerReference.TryGet(out var playerNetworkObject);
            var playerMeshRenderer = playerNetworkObject.GetComponent<PlayerMesh>().PlayerMeshRenderer;
            playerMeshRenderer.material = monsterMaterial;
        }
        
        public void RevertSetMonsterMaterial()
        {
            RevertSetMonsterMaterialRpc(NetworkPlayer.GetLocalInstance().gameObject);
        }
        
        [Rpc(SendTo.Everyone)]
        private void RevertSetMonsterMaterialRpc(NetworkObjectReference playerReference)
        {
            playerReference.TryGet(out var playerNetworkObject);
            var playerMeshRenderer = playerNetworkObject.GetComponent<PlayerMesh>().PlayerMeshRenderer;
            playerMeshRenderer.material = materials[PlayerMaterialIndex];
        }
    }
}