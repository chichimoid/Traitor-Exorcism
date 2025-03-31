using Unity.Netcode;
using UnityEngine;

namespace Maze
{
    public class MazeManager : NetworkBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int length;

        private NetworkVariable<Maze> _maze = new();
        public Maze Maze => _maze.Value;
    
        public static MazeManager Instance { get; private set; }
    
        private void Awake()
        {
            Instance = this;
        }
        public void CreateMaze()
        {
            _maze.Value = new Maze(width, length);
            var generator = new MazeGenerator();
            var mazeValue = _maze.Value;
            mazeValue.Cells = generator.GenerateMaze(width, length);
            _maze.Value = mazeValue;
        }

        public void JoinMaze()
        {
            MazeSpawner.Instance.SpawnMaze();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (NetworkManager.Singleton.IsHost)
            {
                CreateMaze();
            }
            
            JoinMaze();
        }
    }
}