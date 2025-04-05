using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Maze
{
    public class MazeSpawner : NetworkBehaviour
    {
        [SerializeField] private int xOffset;
        [SerializeField] private int yOffset;
        [SerializeField] private int zOffset;
        [SerializeField] private Transform cellPrefab;
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
            var mazeGeneratorCells = MazeManager.Instance.Maze.MazeGeneratorCells;
            
            for (int i = 0; i < mazeGeneratorCells.GetLength(0); ++i)
            {
                for (int j = 0; j < mazeGeneratorCells.GetLength(1); ++j)
                {
                    CellView cellView = Instantiate(cellPrefab, 
                        new Vector3(i * cellSize.x + xOffset, j * cellSize.y + yOffset, j * cellSize.z + zOffset), 
                        Quaternion.identity).GetComponent<CellView>();
                    
                    
                    SpawnBorderWalls(cellView, i, j, mazeGeneratorCells.GetLength(0), mazeGeneratorCells.GetLength(1));
                    
                    cellView.WallLeft.SetActive(mazeGeneratorCells[i, j].wallLeft);
                    cellView.WallBottom.SetActive(mazeGeneratorCells[i, j].wallBottom);

                    Wall leftWallObj = cellView.WallLeft.GetComponent<Wall>();
                    Wall bottomWallObj = cellView.WallBottom.GetComponent<Wall>();
                    
                    if (IsServer)
                    {
                        if (mazeGeneratorCells[i, j].replaceableLeft)
                        {
                            SpawnCellObjects(cellView, "Wall Left");
                        } else
                        {
                            SpawnObjectsOnWall(cellView.WallLeft, "ObjectOnWall1");
                            SpawnObjectsOnWall(cellView.WallLeft, "ObjectOnWall2");
                        }

                        if (mazeGeneratorCells[i, j].replaceableBottom)
                        {
                            SpawnCellObjects(cellView, "Wall Bottom");
                        } else
                        {
                            SpawnObjectsOnWall(cellView.WallBottom, "ObjectOnWall1");
                            SpawnObjectsOnWall(cellView.WallBottom, "ObjectOnWall2");
                        }
                        cellView.DoorLeft.SetActive(mazeGeneratorCells[i, j].doorLeft);
                        cellView.DoorLeft.GetComponent<NetworkObject>().Spawn();
                    
                        cellView.DoorBottom.SetActive(mazeGeneratorCells[i, j].doorBottom);
                        cellView.DoorBottom.GetComponent<NetworkObject>().Spawn();

                        cellView.Lever.SetActive(mazeGeneratorCells[i, j].isLeverRoom);
                        cellView.Lever.GetComponent<NetworkObject>().Spawn();
                    }
                    
                    Renderer floorRender = cellView.Floor.GetComponent<Renderer>();
                    Renderer leftWallRender = cellView.WallLeft.GetComponent<Renderer>();
                    Renderer BottomWallRender = cellView.WallBottom.GetComponent<Renderer>();

                    floorRender.material.color = _zoneColors[mazeGeneratorCells[i, j].zone];
                    // float roomNum = (cells[i, j].roomNumber - 15) / 60f;
                    // Color wallColor = new Color(roomNum * 2, roomNum, roomNum / 2, roomNum);
                    Debug.Log(mazeGeneratorCells[i, j].roomNumber);
                    Debug.Log($"DoorBottom: {mazeGeneratorCells[i, j].doorBottom}, DoorLeft: {mazeGeneratorCells[i, j].doorLeft}");
                    // leftWallRender.material.color = wallColor;
                    // BottomWallRender.material.color = wallColor;
                }
            }
        }

        private void SpawnCellObjects(CellView cellView, string objectName)
        {
            Transform wallSlot = cellView.transform.Find(objectName);

            // GameObject wallObject = cell.walls[UnityEngine.Random.Range(0, cell.walls.Count)];
            GameObject wallObject = GetRandomPrefab(cellView.Walls);
            if (wallObject == null)
            {
                return;
            }

            if (wallSlot.childCount > 0)
                Destroy(wallSlot.GetChild(0).gameObject);

            GameObject spawnedObj = Instantiate(wallObject, wallSlot);
            ApplyRandomTransform(spawnedObj.transform);
            AdjustPrefabScale(wallObject);
            SnapToGroundWithRaycast(wallObject);
            
            spawnedObj.GetComponent<NetworkObject>().Spawn(true);
        }

        private void ApplyRandomTransform(Transform targetTransform)
        {
            float randomYRotation = Random.Range(-30f, 30f);
            targetTransform.Rotate(0f, randomYRotation, 0f, Space.Self);

            float randomXOffset = Random.Range(-1f, 1f);
            float randomZOffset = Random.Range(-1f, 1f);
            targetTransform.Translate(randomXOffset, 0f, randomZOffset, Space.Self);
        }

        private void SpawnObjectsOnWall(GameObject wallHolder, string objectName)
        {
            if (wallHolder == null)
            {
                Debug.Log("Wallholder is null");
                return;
            }

            Wall wall = wallHolder.GetComponentInChildren<Wall>(true);
            if (wall == null)
            {
                Debug.Log("Wall is none");
                return;
            }

            Transform wallSlot = wall.transform.Find(objectName);

            GameObject wallObject = GetRandomPrefab(wall.ObjectsOnWalls);
            if (wallObject == null)
            {
                return;
            }
            var spawnedObj = Instantiate(wallObject, wallSlot);
            
            spawnedObj.GetComponent<NetworkObject>().Spawn(true);
        }

        private void SpawnBorderWalls(CellView cellView, int i, int j, int width, int height)
        {
            if (i == width - 1 && cellView.WallLeft != null)
            {
                GameObject rightWall = Instantiate(cellView.WallLeft, cellView.transform);
                rightWall.transform.localPosition = new Vector3(-cellView.WallLeft.transform.localPosition.x,
                                                             cellView.WallLeft.transform.localPosition.y,
                                                             cellView.WallLeft.transform.localPosition.z);
                rightWall.transform.localRotation = Quaternion.Euler(0, 90, 0);
                rightWall.SetActive(true);
            }

            if (j == height - 1 && cellView.WallBottom != null)
            {
                GameObject topWall = Instantiate(cellView.WallBottom, cellView.transform);
                topWall.transform.localPosition = new Vector3(cellView.WallBottom.transform.localPosition.x,
                                                            cellView.WallBottom.transform.localPosition.y,
                                                            -cellView.WallBottom.transform.localPosition.z);
                topWall.transform.localRotation = Quaternion.Euler(0, 180, 0);
                topWall.SetActive(true);
            }
        }

        GameObject GetRandomPrefab(List<WeightedPrefab> weightedPrefabs)
        {
            float totalWeight = 0;
            foreach (var wp in weightedPrefabs)
                totalWeight += wp.weight;

            float randomPoint = Random.Range(0, totalWeight);

            foreach (var wp in weightedPrefabs)
            {
                if (randomPoint < wp.weight)
                    return wp.prefab;
                randomPoint -= wp.weight;
            }

            return weightedPrefabs[0].prefab;
        }

        private void AdjustPrefabScale(GameObject prefabInstance, float minSize = 0.84f)
        {
            Renderer rend = prefabInstance.GetComponent<Renderer>();
            if (rend == null)
            {
                rend = prefabInstance.GetComponentInChildren<Renderer>();
                if (rend == null) return;
            }

            Bounds bounds = rend.bounds;
            Vector3 size = bounds.size;

            float maxUnderSize = Mathf.Min(size.x, size.y, size.z);
            Debug.Log($"Size: {maxUnderSize}");
            if (maxUnderSize < minSize)
            {
                float scaleFactor = minSize / maxUnderSize;
                prefabInstance.transform.localScale *= scaleFactor;
            }
        }

        void SnapToGroundWithRaycast(GameObject obj)
        {
            Renderer renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer == null) return;

            Bounds bounds = renderer.bounds;
            float rayLength = 10f;
            Vector3 rayStart = obj.transform.position + Vector3.up * 2f;

            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, rayLength))
            {
                float bottomOffset = bounds.min.y - obj.transform.position.y;
                obj.transform.position = hit.point - new Vector3(0, bottomOffset, 0);
            }
        }
    }
}
