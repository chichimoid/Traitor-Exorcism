using PlayerScripts;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Maze
{
    public class MazeManager : NetworkBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int length;
        [SerializeField] private Transform mazeSpawnerPrefab;
        
        private readonly NetworkVariable<MazeData> _mazeData = new();
        public MazeData MazeData => _mazeData.Value;
    
        public static MazeManager Instance { get; private set; }
    
        private void Awake()
        {
            Instance = this;
        }
        
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
        public void PlayerSpawnedServerRpc()
        {
            ++_spawnedPlayers.Value;

            if (_spawnedPlayers.Value == NetworkManager.Singleton.ConnectedClients.Count)
            {
                CreateMazeServerRpc();
            }
        }
        
        [Rpc(SendTo.Server)]
        private void CreateMazeServerRpc()
        {
            _mazeData.Value = new MazeData(width, length);
            var generator = new MazeGenerator();
            var mazeValue = _mazeData.Value;
            mazeValue.MazeGeneratorCells = generator.GenerateMaze(width, length);
            _mazeData.Value = mazeValue;
            
            var spawnedObj = Instantiate(mazeSpawnerPrefab);
            spawnedObj.GetComponent<NetworkObject>().Spawn(true);
            spawnedObj.GetComponent<MazeSpawner>().SpawnMaze(_mazeData.Value);
            
            FinishSpawnRpc();
        }
        
        [Rpc(SendTo.Everyone)]
        private void FinishSpawnRpc()
        {
            PlayerLocker.Instance.UnlockPhysics();
            PlayerLocker.Instance.UnlockRotation();
            PlayerLocker.Instance.UnlockMovement();
        }
    }
}