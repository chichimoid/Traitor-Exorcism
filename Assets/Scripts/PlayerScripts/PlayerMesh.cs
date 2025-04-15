using System;
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
        
        private readonly NetworkList<int> _excludedMaterials = new(new List<int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
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

            if (!IsServer)
            {
                foreach (var material in NetworkPlayer.GetInstance(0).GetComponent<PlayerMesh>()._excludedMaterials)
                {
                    _excludedMaterials.Add(material);
                }
            }

            var availableIndices = Enumerable.Range(0, materials.Count).ToList();
            foreach (var excludedMaterial in _excludedMaterials)
            {
                availableIndices.Remove(excludedMaterial);
            }
            int availableIndexIndex = Random.Range(0, availableIndices.Count);
            int index = availableIndices[availableIndexIndex];

            SetOtherPlayersMaterials();
            
            PlayerMaterialIndex = index;
            
            PlayerMeshRenderer.material = materials[index];

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

        [Rpc(SendTo.NotMe)]
        private void SetPlayerMaterialNotMeRpc(NetworkObjectReference playerReference, int index)
        {
            playerReference.TryGet(out var playerNetworkObject);
            var playerMesh = playerNetworkObject.GetComponent<PlayerMesh>();
            playerMesh.PlayerMeshRenderer.material = materials[index];
            
            NetworkPlayer.GetLocalInstance().GetComponent<PlayerMesh>()._excludedMaterials.Add(index);
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