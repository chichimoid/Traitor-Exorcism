using System;
using NetworkHelperScripts;
using PlayerScripts;
using PlayerScripts.UI;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Voting.GameCycle;

namespace Voting
{
    public class VotingSceneInitializer : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private Vector2 votingCircleCenter;
        [SerializeField] private float votingCircleRadius;
        [SerializeField] private Vector3 voteDisplayerLocalSpawnOffset;
        
        [Header("References")]
        [SerializeField] private VotingPhaseInitializer votingPhaseInitializer;
        [SerializeField] private VotingPhaseEnder votingPhaseEnder;
        [SerializeField] private AftermathPhaseInitializer aftermathPhaseInitializer;
        
        [Header("Prefabs")]
        [SerializeField] private Transform voteManagerPrefab;
        [SerializeField] private Transform voteDisplayerPrefab;
        
        private readonly NetworkVariable<int> _spawnedPlayers = new(0);
        private readonly NetworkVariable<int> _initializedPlayers = new(0);
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            PlayerLocker.Instance.LockMovement();
            PlayerLocker.Instance.LockRotation();
            PlayerLocker.Instance.LockPhysics();
            
            PlayerSpawnedServerRpc();
        }

        [Rpc(SendTo.Server)]
        public void PlayerSpawnedServerRpc()
        {
            ++_spawnedPlayers.Value;

            if (_spawnedPlayers.Value == NetworkManager.Singleton.ConnectedClients.Count)
            {
                InitScene();
            }
        }
        private void InitScene()
        {
            InitPlayers();
        }
        
        public void InitPlayers()
        {
            var len = NetworkManager.Singleton.ConnectedClients.Count;
            var spawnedNetworkObject = Instantiate(voteManagerPrefab).GetComponent<NetworkObject>();
            spawnedNetworkObject.Spawn(true);
            
            ResetMonsterTargetRpc(RpcTarget.Single(GameManager.GetMonsterId(), RpcTargetUse.Temp));
            
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
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void InitClientVotingTargetRpc(int i, int len, NetworkObjectReference voteDisplayerReference, RpcParams rpcParams)
        {
            var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            playerObject.GetComponent<PlayerHealth>().enabled = false;
            playerObject.GetComponent<PlayerInfection>().enabled = false;
            PlayerLocker.Instance.LockActions();
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
            var playerPointOnVotingCircle = GetPlayerPointOnVotingCircle(i, len);
            var playerRb = playerObject.GetComponent<Rigidbody>();
            playerRb.position = new Vector3(playerPointOnVotingCircle.x, playerRb.position.y, playerPointOnVotingCircle.y);
            playerObject.transform.LookAt(new Vector3(votingCircleCenter.x, playerObject.transform.position.y, votingCircleCenter.y));
            
            PlayerInitializedServerRpc();
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
        private void ResetMonsterTargetRpc(RpcParams rpcParams)
        {
            var networkPlayer = NetworkPlayer.GetLocalInstance();
            networkPlayer.GetComponent<MonsterBar>().enabled = false;
            var playerMesh = networkPlayer.GetComponent<PlayerMesh>();
            playerMesh.RevertSetMonsterMaterial();
        }
        
        [Rpc(SendTo.Server)]
        public void PlayerInitializedServerRpc()
        {
            ++_initializedPlayers.Value;

            if (_initializedPlayers.Value == NetworkManager.Singleton.ConnectedClients.Count)
            {
                GameManager.Instance.OnVotingSceneStarted(votingPhaseInitializer, votingPhaseEnder, aftermathPhaseInitializer);
                Destroy(gameObject);
                FinishInitSceneRpc();
            }
        }

        [Rpc(SendTo.Everyone)]
        private void FinishInitSceneRpc()
        {
            PlayerLocker.Instance.UnlockPhysics();
            PlayerLocker.Instance.UnlockRotation();
        }
    }
}