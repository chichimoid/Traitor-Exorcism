using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Cinemachine.CinemachineSplineRoll;

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
                    SpawnBorderWalls(cell, i, j, cells.GetLength(0), cells.GetLength(1));

                    cell.wallLeft.SetActive(cells[i, j].wallLeft);
                    cell.wallBottom.SetActive(cells[i, j].wallBottom);

                    Wall leftWallObj = cell.wallLeft.GetComponent<Wall>();
                    Wall bottomWallObj = cell.wallBottom.GetComponent<Wall>();

                    if (cells[i, j].replaceableLeft)
                    {
                        SpawnCellObjects(cell, "Wall Left");
                    } else
                    {
                        SpawnObjectsOnWall(cell.wallLeft, "ObjectOnWall1");
                        SpawnObjectsOnWall(cell.wallLeft, "ObjectOnWall2");
                    }

                    if (cells[i, j].replaceableBottom)
                    {
                        SpawnCellObjects(cell, "Wall Bottom");
                    } else
                    {
                        SpawnObjectsOnWall(cell.wallBottom, "ObjectOnWall1");
                        SpawnObjectsOnWall(cell.wallBottom, "ObjectOnWall2");
                    }

                    cell.doorLeft.SetActive(cells[i, j].doorLeft);
                    cell.doorBottom.SetActive(cells[i, j].doorBottom);

                    cell.lever.SetActive(cells[i, j].isLeverRoom);
                    
                    Renderer floorRender = cell.floor.GetComponent<Renderer>();
                    Renderer leftWallRender = cell.wallLeft.GetComponent<Renderer>();
                    Renderer BottomWallRender = cell.wallBottom.GetComponent<Renderer>();

                    floorRender.material.color = _zoneColors[cells[i, j].zone];
                    // float roomNum = (cells[i, j].roomNumber - 15) / 60f;
                    // Color wallColor = new Color(roomNum * 2, roomNum, roomNum / 2, roomNum);
                    Debug.Log(cells[i, j].roomNumber);
                    Debug.Log($"DoorBottom: {cells[i, j].doorBottom}, DoorLeft: {cells[i, j].doorLeft}");
                    // leftWallRender.material.color = wallColor;
                    // BottomWallRender.material.color = wallColor;
                }
            }
        }

        private void SpawnCellObjects(Cell cell, string objectName)
        {
            Transform wallSlot = cell.transform.Find(objectName);

            // GameObject wallObject = cell.walls[UnityEngine.Random.Range(0, cell.walls.Count)];
            GameObject wallObject = GetRandomPrefab(cell.walls);
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
            Instantiate(wallObject, wallSlot);
        }

        private void SpawnBorderWalls(Cell cell, int i, int j, int width, int height)
        {
            if (i == width - 1 && cell.wallLeft != null)
            {
                GameObject rightWall = Instantiate(cell.wallLeft, cell.transform);
                rightWall.transform.localPosition = new Vector3(-cell.wallLeft.transform.localPosition.x,
                                                             cell.wallLeft.transform.localPosition.y,
                                                             cell.wallLeft.transform.localPosition.z);
                rightWall.transform.localRotation = Quaternion.Euler(0, 90, 0);
                rightWall.SetActive(true);
            }

            if (j == height - 1 && cell.wallBottom != null)
            {
                GameObject topWall = Instantiate(cell.wallBottom, cell.transform);
                topWall.transform.localPosition = new Vector3(cell.wallBottom.transform.localPosition.x,
                                                            cell.wallBottom.transform.localPosition.y,
                                                            -cell.wallBottom.transform.localPosition.z);
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
