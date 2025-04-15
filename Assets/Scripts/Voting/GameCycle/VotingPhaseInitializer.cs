using System;
using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace Voting.GameCycle
{
    public class VotingPhaseInitializer : NetworkBehaviour
    {
        [Header("Configure")] 
        [SerializeField] private Vector2 votingCircleCenter;
        [SerializeField] private float votingCircleRadius;
        [SerializeField] private Vector3 voteDisplayerLocalSpawnOffset;
        [SerializeField] private int votingTimeSeconds;
        
        [Header("References")]
        [SerializeField] private ServerTimer serverTimer;
        
        [Header("Prefabs")]
        [SerializeField] private Transform voteManagerPrefab;
        [SerializeField] private Transform voteDisplayerPrefab;

        public void Init()
        {
            var len = NetworkManager.Singleton.ConnectedClients.Count;
            var spawnedNetworkObject = Instantiate(voteManagerPrefab).GetComponent<NetworkObject>();
            spawnedNetworkObject.Spawn(true);
            
            SetMonsterMeshBackTargetRpc(RpcTarget.Single(GameManager.GetMonsterId(), RpcTargetUse.Temp));
            
            for (int i = 0; i < len; i++)
            {
                ulong id = NetworkManager.Singleton.ConnectedClientsIds[i];
                if (GameManager.Instance.AlivePlayersIds.Contains(id))
                {
                    spawnedNetworkObject = Instantiate(voteDisplayerPrefab).GetComponent<NetworkObject>();
                    spawnedNetworkObject.Spawn(true);
                    spawnedNetworkObject.GetComponent<VoteDisplayer>().BoundId = id;
                }
                
                InitClientVotingTargetRpc(i, len, spawnedNetworkObject, RpcTarget.Single(id, RpcTargetUse.Temp));
            }
            
            serverTimer.StartTimer(votingTimeSeconds);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void InitClientVotingTargetRpc(int i, int len, NetworkObjectReference voteDisplayerReference, RpcParams rpcParams)
        {
            PlayerLocker.Instance.LockMovement();
            
            var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            TeleportPlayerToVotingCircle(playerObject, i, len);
            
            if (GameManager.Instance.AlivePlayersIds.Contains((ulong)i))
            {
                voteDisplayerReference.TryGet(out var voteDisplayerObject);
                SetFollowTransformRpc(voteDisplayerObject, playerObject);
            
                playerObject.GetComponent<PlayerVoter>().enabled = true;
            }
        }

        private void TeleportPlayerToVotingCircle(NetworkObject playerObject, int i, int len)
        {
            PlayerLocker.Instance.LockRotation();
            PlayerLocker.Instance.LockPhysics();

            var playerPointOnVotingCircle = GetPlayerPointOnVotingCircle(i, len);
            var playerRb = playerObject.GetComponent<Rigidbody>();
            playerRb.position = new Vector3(playerPointOnVotingCircle.x, playerRb.position.y, playerPointOnVotingCircle.y);
            playerObject.transform.LookAt(new Vector3(votingCircleCenter.x, playerObject.transform.position.y, votingCircleCenter.y));
            
            PlayerLocker.Instance.UnlockPhysics();
            PlayerLocker.Instance.UnlockRotation();

        }

        private Vector2 GetPlayerPointOnVotingCircle(int i, int len)
        {
            return new Vector2(
                votingCircleCenter.x + (float)Math.Sin(Math.PI * 2 * i / (float)len) * votingCircleRadius,
                votingCircleCenter.y + (float)Math.Cos(Math.PI * 2 * i / (float)len) * votingCircleRadius);
        }
        
        [Rpc(SendTo.Everyone)]
        private void SetFollowTransformRpc(NetworkObjectReference followerReference, NetworkObjectReference targetReference)
        {
            followerReference.TryGet(out NetworkObject follower);
            targetReference.TryGet(out NetworkObject target);
            FollowTransformManager.Instance.Follow(follower.transform, target.transform);
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void SetMonsterMeshBackTargetRpc(RpcParams rpcParams)
        {
            var networkPlayer = NetworkPlayer.GetLocalInstance();
            var playerMesh = networkPlayer.GetComponent<PlayerMesh>();
            playerMesh.RevertSetMonsterMaterial();
        }
    }
}