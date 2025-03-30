using UnityEngine;

namespace Maze
{
    public class MazeSpawner : MonoBehaviour
    {
        [SerializeField] private int xOffset;
        [SerializeField] private int yOffset;
        [SerializeField] private int zOffset;
        [SerializeField] private CellSO cellSO;
        [SerializeField] private Vector3 cellSize;
        
        private Color[] _zoneColors = {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
        };
    
        public static MazeSpawner Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    
        public void SpawnMaze()
        {
            var cells = MazeManager.Instance.Maze.Cells;
            
            for (int i = 0; i < cells.GetLength(0); ++i)
            {
                for (int j = 0; j < cells.GetLength(1); ++j)
                {   
                    Cell cell = Instantiate(cellSO.prefab, 
                        new Vector3(i * cellSize.x + xOffset, j * cellSize.y + yOffset, j * cellSize.z + zOffset), 
                        Quaternion.identity).GetComponent<Cell>();
                    cell.wallLeft.SetActive(cells[i, j].wallLeft);
                    cell.wallBottom.SetActive(cells[i, j].wallBottom);
                    
                    Renderer floorRender = cell.floor.GetComponent<Renderer>();

                    floorRender.material.color = _zoneColors[cells[i, j].zone];
                }
            }
        }
    }
}
