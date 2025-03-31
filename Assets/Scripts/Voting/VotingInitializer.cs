using System;
using PlayerScripts;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Voting
{
    public class VotingInitializer : NetworkBehaviour
    {
        [Header("Configure")] 
        [SerializeField] private Vector2 votingCircleCenter;
        [SerializeField] private float votingCircleRadius;
        [SerializeField] private Vector3 voteDisplayerLocalSpawnOffset;
        [Header("References")]
        [SerializeField] private Transform votingDisplayerSpawnerPrefab;
        [SerializeField] private Transform voteManagerPrefab;
        [SerializeField] private Transform voteDisplayerPrefab;

        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                StartVotingServerRpc();
            }
        }
        
        [Rpc(SendTo.Server)]
        private void StartVotingServerRpc()
        {
            var len = NetworkManager.Singleton.ConnectedClients.Count;
            NetworkObject spawnedNetworkObject;
            for (int i = 0; i < len; i++)
            {
                spawnedNetworkObject = Instantiate(voteDisplayerPrefab).GetComponent<NetworkObject>();
                spawnedNetworkObject.Spawn(true);
                
                StartVotingTeleportRpc(i, len, spawnedNetworkObject, RpcTarget.Single(NetworkManager.Singleton.ConnectedClientsIds[i], RpcTargetUse.Temp));
            }

            spawnedNetworkObject  = Instantiate(voteManagerPrefab).GetComponent<NetworkObject>();
            spawnedNetworkObject.Spawn(true);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void StartVotingTeleportRpc(int i, int len, NetworkObjectReference voteDisplayerReference, RpcParams rpcParams)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>().State = PlayerState.InVoting;
            
            Debug.Log("Starting voting teleport");
            var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            var playerRb = playerObject.GetComponent<Rigidbody>();
            playerRb.position = new Vector3(
                votingCircleCenter.x + (float)Math.Sin(Math.PI * 2 * i / (float)len) * votingCircleRadius,
                playerRb.position.y,
                votingCircleCenter.y + (float)Math.Cos(Math.PI * 2 * i / (float)len) * votingCircleRadius);
            playerObject.transform.LookAt(new Vector3(votingCircleCenter.x, playerRb.position.y, votingCircleCenter.y));
            
            
            voteDisplayerReference.TryGet(out var voteDisplayerObject);
            
            FollowTransformManager.Instance.Follow(voteDisplayerObject.transform, playerObject.transform);
            
            voteDisplayerObject.GetComponent<VoteDisplayer>().BoundId = NetworkManager.Singleton.LocalClientId;
        }
    }
}