using Maze.GameCycle;
using PlayerScripts;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Voting;

namespace Maze
{
    public class MazeSceneInitializer : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private int width;
        [SerializeField] private int length;
        [Header("References")]
        [SerializeField] private Phase1Initializer phase1Initializer;
        [SerializeField] private Phase1Ender phase1Ender;
        [SerializeField] private Phase2Initializer phase2Initializer;
        [SerializeField] private Phase2Ender phase2Ender;
        [SerializeField] private Phase3Initializer phase3Initializer;
        [SerializeField] private Phase3Ender phase3Ender;

        [Header("Prefabs")]
        [SerializeField] private Transform mazeSpawnerPrefab;
        
        private readonly NetworkVariable<MazeData> _mazeData = new();
        public MazeData MazeData => _mazeData.Value;
        
        private readonly NetworkVariable<int> _spawnedPlayers = new(0);
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            PlayerLocker.Instance.LockMovement();
            PlayerLocker.Instance.LockRotation();
            PlayerLocker.Instance.LockPhysics();
            
            PlayerSpawnedServerRpc();
        }

        [Rpc(SendTo.Server)]
        private void PlayerSpawnedServerRpc()
        {
            ++_spawnedPlayers.Value;

            if (_spawnedPlayers.Value == NetworkManager.Singleton.ConnectedClients.Count)
            {
                InitScene();
            }
        }
        
        private void InitScene()
        {
            _mazeData.Value = new MazeData(width, length);
            var generator = new MazeGenerator();
            var mazeValue = _mazeData.Value;
            mazeValue.MazeGeneratorCells = generator.GenerateMaze(width, length);
            _mazeData.Value = mazeValue;
            
            var spawnedObj = Instantiate(mazeSpawnerPrefab);
            spawnedObj.GetComponent<NetworkObject>().Spawn(true);
            spawnedObj.GetComponent<MazeSpawner>().SpawnMaze(_mazeData.Value);
            
            GameManager.Instance.OnMazeSceneStarted(phase1Initializer, phase1Ender, phase2Initializer, phase2Ender, phase3Initializer, phase3Ender);
            
            FinishInitSceneRpc();
        }
        
        [Rpc(SendTo.Everyone)]
        private void FinishInitSceneRpc()
        {
            PlayerLocker.Instance.UnlockPhysics();
            PlayerLocker.Instance.UnlockRotation();
            PlayerLocker.Instance.UnlockMovement();
        }
    }
}