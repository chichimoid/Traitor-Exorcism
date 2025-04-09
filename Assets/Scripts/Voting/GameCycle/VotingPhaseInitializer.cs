using System;
using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Voting
{
    public class VotingPhaseInitializer : NetworkBehaviour
    {
        [Header("Configure")] 
        [SerializeField] private Vector2 votingCircleCenter;
        [SerializeField] private float votingCircleRadius;
        [SerializeField] private Vector3 voteDisplayerLocalSpawnOffset;
        [SerializeField] private int votingTimeSeconds;

        [FormerlySerializedAs("timer")]
        [Header("References")]
        [SerializeField] private ServerTimer serverTimer;
        
        [Header("Prefabs")]
        [SerializeField] private Transform voteManagerPrefab;
        [SerializeField] private Transform voteDisplayerPrefab;

        public void StartVotingPhase()
        {
            var len = NetworkManager.Singleton.ConnectedClients.Count;
            var spawnedNetworkObject = Instantiate(voteManagerPrefab).GetComponent<NetworkObject>();
            spawnedNetworkObject.Spawn(true);
            
            serverTimer.StartTimer(votingTimeSeconds);
            
            for (int i = 0; i < len; i++)
            {
                spawnedNetworkObject = Instantiate(voteDisplayerPrefab).GetComponent<NetworkObject>();
                spawnedNetworkObject.Spawn(true);
                
                InitClientVotingRpc(i, len, spawnedNetworkObject, RpcTarget.Single(NetworkManager.Singleton.ConnectedClientsIds[i], RpcTargetUse.Temp));
                
                spawnedNetworkObject.GetComponent<VoteDisplayer>().BoundId = NetworkManager.Singleton.ConnectedClientsIds[i];
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void InitClientVotingRpc(int i, int len, NetworkObjectReference voteDisplayerReference, RpcParams rpcParams)
        {
            PlayerLocker.Instance.LockMovement();
            
            var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            TeleportPlayerToVotingCircle(playerObject, i, len);
            
            voteDisplayerReference.TryGet(out var voteDisplayerObject);
            SetFollowTransformRpc(voteDisplayerObject, playerObject);
            
            playerObject.GetComponent<PlayerVoter>().enabled = true;
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
    }
}