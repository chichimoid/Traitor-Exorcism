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
                        SpawnCellObjects(cell, "Wall Left", cell.walls);
                    } else
                    {
                        SpawnObjectsOnWall(cell.wallLeft, "ObjectOnWall1");
                        SpawnObjectsOnWall(cell.wallLeft, "ObjectOnWall2");
                    }

                    if (cells[i, j].replaceableBottom)
                    {
                        SpawnCellObjects(cell, "Wall Bottom", cell.walls);
                    } else
                    {
                        SpawnObjectsOnWall(cell.wallBottom, "ObjectOnWall1");
                        SpawnObjectsOnWall(cell.wallBottom, "ObjectOnWall2");
                    }

                    SpawnCellObjects(cell, "ItemSlot1", cell.items);
                    SpawnCellObjects(cell, "ItemSlot2", cell.items);

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


        private void SpawnCellObjects(Cell cell, string objectName, List<WeightedPrefab> objList, bool scale=true)
        {
            Transform wallSlot = cell.transform.Find(objectName);
            Debug.Log($"Spawning in {objectName}");
            GameObject wallObject = GetRandomPrefab(objList);
            if (wallObject == null)
            {
                return;
            }

            if (wallSlot.childCount > 0)
                Destroy(wallSlot.GetChild(0).gameObject);

            //GameObject spawnedObj = Instantiate(wallObject, wallSlot);
            //ApplyRandomTransform(spawnedObj.transform);

            GameObject spawnedObj = Instantiate(wallObject, wallSlot);
            Debug.Log($"My position is {spawnedObj.transform.localPosition}");
            // spawnedObj.transform.localPosition = Vector3.zero; // Сбрасываем позицию
            // spawnedObj.transform.localRotation = Quaternion.identity; // Сбрасываем поворот
            ApplyRandomTransform(spawnedObj.transform);

            if (scale)
            {
                AdjustPrefabScale(spawnedObj);
            }
            // SnapToGroundWithRaycast(spawnedObj);
        }

        private void ApplyRandomTransform(Transform targetTransform)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(targetTransform))
            {
                Debug.LogError("Пытаетесь изменить оригинальный префаб! Используйте Instantiate.");
                return;
            }
            Vector3 originalPosition = targetTransform.position;
            Quaternion originalRotation = targetTransform.rotation;

            float randomYRotation = Random.Range(-30f, 30f);
            targetTransform.Rotate(0f, randomYRotation, 0f, Space.World);

            Vector3 localOffset = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            );

            targetTransform.localPosition += localOffset;
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
            if (PrefabUtility.IsPartOfPrefabAsset(prefabInstance))
            {
                Debug.LogError("Пытаетесь изменить оригинальный префаб! Используйте Instantiate.");
                return;
            }
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
            Vector3 rayStart = bounds.center;
            float rayLength = bounds.extents.y + 2f;

            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, rayLength))
            {
                // Смещение от центра до нижней грани
                float bottomOffset = bounds.center.y - bounds.min.y;
                obj.transform.position = hit.point + new Vector3(0, bottomOffset, 0);
            }
            else
            {
                Debug.LogWarning($"Объект {obj.name} не приземлился!");
            }
        }
    }
}
