using Maze;
using Unity.Netcode;
using UnityEngine;
using Voting.GameCycle;

namespace Voting
{
    public class VotingSceneInitializer : NetworkBehaviour
    {
        [SerializeField] private VotingPhaseInitializer votingPhaseInitializer;
        [SerializeField] private VotingPhaseEnder votingPhaseEnder;
        
        private readonly NetworkVariable<int> _spawnedPlayers = new(0);
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            PlayerSpawnedServerRpc();
        }

        [Rpc(SendTo.Server)]
        public void PlayerSpawnedServerRpc()
        {
            ++_spawnedPlayers.Value;

            if (_spawnedPlayers.Value == NetworkManager.Singleton.ConnectedClients.Count)
            {
                GameManager.Instance.OnVotingSceneStarted(votingPhaseInitializer, votingPhaseEnder);
            }
        }
    }
}