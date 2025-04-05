using Unity.Netcode;
using UnityEngine;

namespace Maze
{
    public class MazeManager : NetworkBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int length;
        [SerializeField] private Transform mazeSpawnerPrefab;

        private readonly NetworkVariable<Maze> _localMaze = new();
        public Maze Maze => _localMaze.Value;
        public CellView MazeView { get; private set; }
    
        public static MazeManager Instance { get; private set; }
    
        private void Awake()
        {
            Instance = this;
        }
        public void CreateMaze()
        {
            _localMaze.Value = new Maze(width, length);
            var generator = new MazeGenerator();
            var mazeValue = _localMaze.Value;
            mazeValue.MazeGeneratorCells = generator.GenerateMaze(width, length);
            _localMaze.Value = mazeValue;
        }

        public void SpawnMaze()
        {
            MazeSpawner.Instance.SpawnMaze();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                var spawnedObj = Instantiate(mazeSpawnerPrefab);
                spawnedObj.GetComponent<NetworkObject>().Spawn(true);
                
                CreateMaze();
            }
            
            SpawnMaze();
        }
    }
}