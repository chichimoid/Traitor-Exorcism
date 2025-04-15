using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Maze
{
    public class MazeSpawner : NetworkBehaviour
    {
        [SerializeField] private int xOffset;
        [SerializeField] private int yOffset;
        [SerializeField] private int zOffset;
        [SerializeField] private Transform cellPrefab;
        [SerializeField] private Vector3 cellSize;
        [SerializeField] private Transform doorPrefab;
        [SerializeField] private Transform leverPrefab;
        [SerializeField] private Transform wallPrefab;
        
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
    
        public void SpawnMaze(MazeData mazeData)
        {
            var cells = mazeData.MazeGeneratorCells;
            
            for (int i = 0; i < cells.GetLength(0); ++i)
            {
                for (int j = 0; j < cells.GetLength(1); ++j)
                {   
                    Cell cell = Instantiate(cellPrefab, 
                        new Vector3(i * cellSize.x + xOffset, j * cellSize.y + yOffset, j * cellSize.z + zOffset), 
                        Quaternion.identity).GetComponent<Cell>();
                    cell.GetComponent<NetworkObject>().Spawn(true);
                    
                    SpawnBorderWallsRpc(cell.GetComponent<NetworkObject>(), i, j, cells.GetLength(0), cells.GetLength(1));

                    if (!cells[i, j].wallLeft)
                    {
                        DestroyWallRpc(cell.GetComponent<NetworkObject>(), WallHolder.Left);
                    }

                    if (!cells[i, j].wallBottom)
                    {
                        DestroyWallRpc(cell.GetComponent<NetworkObject>(), WallHolder.Bottom);
                    }

                    Maze.Wall leftWallObj = cell.wallLeft.GetComponent<Maze.Wall>();
                    Maze.Wall bottomWallObj = cell.wallBottom.GetComponent<Maze.Wall>();

                    if (cells[i, j].replaceableLeft)
                    {
                        SpawnCellObjects(cell, "Wall Left", CellObjectList.Walls, false);
                    } else
                    {
                        SpawnObjectsOnWall(cell, WallHolder.Left, "ObjectOnWall1");
                        SpawnObjectsOnWall(cell, WallHolder.Left, "ObjectOnWall2");
                    }

                    if (cells[i, j].replaceableBottom)
                    {
                        SpawnCellObjects(cell, "Wall Bottom", CellObjectList.Walls, false);
                    } else
                    {
                        SpawnObjectsOnWall(cell, WallHolder.Bottom, "ObjectOnWall1");
                        SpawnObjectsOnWall(cell, WallHolder.Bottom, "ObjectOnWall2");
                    }

                    SpawnCellObjects(cell, "ItemSlot1", CellObjectList.Items, false, false);
                    SpawnCellObjects(cell, "ItemSlot2", CellObjectList.Items, false, false);

                    if (cells[i, j].doorLeft && cells[i, j].replaceableLeft)
                    {
                        var spawnedObj = Instantiate(doorPrefab, cell.doorLeft.transform.position, cell.doorLeft.transform.rotation);
                        spawnedObj.transform.localScale = cell.doorLeft.transform.localScale;
                        spawnedObj.GetComponent<NetworkObject>().Spawn(true);
                    }
                    else
                    {
                        Destroy(cell.doorLeft);
                    }
                    
                    if (cells[i, j].doorBottom && cells[i, j].replaceableBottom)
                    {
                        var spawnedObj = Instantiate(doorPrefab, cell.doorBottom.transform.position, cell.doorBottom.transform.rotation);
                        spawnedObj.transform.localScale = cell.doorBottom.transform.localScale;
                        spawnedObj.GetComponent<NetworkObject>().Spawn(true);
                    }
                    else
                    {
                        Destroy(cell.doorBottom);
                    }
                    
                    if (cells[i, j].isLeverRoom)
                    {
                        var spawnedObj = Instantiate(leverPrefab, cell.lever.transform.position, cell.lever.transform.rotation);
                        spawnedObj.transform.localScale = cell.lever.transform.localScale;
                        spawnedObj.GetComponent<NetworkObject>().Spawn(true);
                    }
                    else
                    {
                        Destroy(cell.lever);
                    }

                    SetFloorColorRpc(cell.GetComponent<NetworkObject>(), mazeData, i, j);
                    // float roomNum = (cells[i, j].roomNumber - 15) / 60f;
                    // Color wallColor = new Color(roomNum * 2, roomNum, roomNum / 2, roomNum);
                    Debug.Log(cells[i, j].roomNumber);
                    Debug.Log($"DoorBottom: {cells[i, j].doorBottom}, DoorLeft: {cells[i, j].doorLeft}");
                    // leftWallRender.material.color = wallColor;
                    // BottomWallRender.material.color = wallColor;
                }
            }
        }
        [Rpc(SendTo.Everyone)]
        private void DestroyWallRpc(NetworkObjectReference cellReference, WallHolder wallEnum)
        {
            cellReference.TryGet(out var cellObj);
            var cell = cellObj.GetComponent<Cell>();
            
            Destroy(wallEnum.ToWallObject(cell));
        }

        [Rpc(SendTo.Everyone)]
        private void SetFloorColorRpc(NetworkObjectReference cellReference, MazeData mazeData, int i, int j)
        {
            cellReference.TryGet(out var cellObj);
            var cell = cellObj.GetComponent<Cell>();
            Renderer floorRender = cell.floor.GetComponent<Renderer>();
            Renderer leftWallRender = cell.wallLeft.GetComponent<Renderer>();
            Renderer BottomWallRender = cell.wallBottom.GetComponent<Renderer>();
            var cells = mazeData.MazeGeneratorCells;
            floorRender.material.color = _zoneColors[cells[i, j].zone];
        }
        
        private void SpawnCellObjects(Cell cell, string objectName, CellObjectList objListEnum, bool scale = true, bool spawnAsChild = true)
        {
            List<WeightedPrefab> objList = objListEnum.ToWeightedPrefabList(cell);
            
            Transform wallSlot = cell.transform.Find(objectName);
            Debug.Log($"Spawning in {objectName}");
            int wallObjectIndex = GetRandomPrefabIndex(objList);
            var wallObject = objList[wallObjectIndex].prefab;
            if (wallObject == null)
            {
                return;
            }

            if (wallSlot.childCount > 0)
                DestroyWallSlotObjRpc(cell.GetComponent<NetworkObject>(), objectName);

            //GameObject spawnedObj = Instantiate(wallObject, wallSlot);
            //ApplyRandomTransform(spawnedObj.transform);
            
            if (spawnAsChild)
            {
                SpawnCellObjectsRpc(cell.GetComponent<NetworkObject>(), objListEnum, objectName, wallObjectIndex);
            }
            else
            {
                var spawnedObj = Instantiate(wallObject, wallSlot.transform.position, wallSlot.transform.rotation);
                AlignToPlaceholder(wallSlot, spawnedObj);
                spawnedObj.GetComponent<NetworkObject>().Spawn(true);
            }
        }
        
        [Rpc(SendTo.Everyone)]
        private void SpawnCellObjectsRpc(NetworkObjectReference cellReference, CellObjectList objListEnum, FixedString64Bytes objectName64Bytes, int wallObjectIndex, bool scale = true)
        {
            cellReference.TryGet(out var cellObj);
            var cell = cellObj.GetComponent<Cell>();

            var objList = objListEnum.ToWeightedPrefabList(cell);
            
            string objectName = objectName64Bytes.ToString();
            Transform wallSlot = cell.transform.Find(objectName);
            var wallObject = objList[wallObjectIndex].prefab;
            var spawnedObj = Instantiate(wallObject, wallSlot);
            
            Debug.Log($"My position is {spawnedObj.transform.localPosition}");
            ApplyRandomTransform(spawnedObj.transform);

            if (scale)
            {
                AdjustPrefabScale(spawnedObj);
            }
            // SnapToGroundWithRaycast(spawnedObj);
        }

        void AlignToPlaceholder(Transform placeholder, GameObject targetObj, bool rotate = false)
        {
            if (placeholder == null || targetObj == null) return;

            // Получаем рендерер или коллайдер для расчёта нижней точки
            Renderer renderer = targetObj.GetComponent<Renderer>();
            if (renderer == null) return;

            Bounds bounds = renderer.bounds;

            // Вычисляем вертикальное смещение: разница между пивотом объекта и нижней точкой меша по Y
            float verticalOffset = targetObj.transform.position.y - bounds.min.y;

            // Сохраняем текущие X и Z объекта, меняем только Y
            Vector3 newPosition = new Vector3(
                targetObj.transform.position.x,          // Сохраняем X
                placeholder.position.y + verticalOffset, // Новый Y
                targetObj.transform.position.z           // Сохраняем Z
            );

            targetObj.transform.position = newPosition;
            if (rotate)
            {
                targetObj.transform.rotation = placeholder.rotation;
            }
        }

        [Rpc(SendTo.Everyone)]
        private void DestroyWallSlotObjRpc(NetworkObjectReference cellReference, FixedString64Bytes objectName64Bytes)
        {
            cellReference.TryGet(out var cellObj);
            Transform wallSlot = cellObj.transform.Find(objectName64Bytes.ToString());
            Destroy(wallSlot.GetChild(0).gameObject);
        }

        private void ApplyRandomTransform(Transform targetTransform)
        {
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
            //if (IsCollidingWithAnything(targetTransform))
            //{
            //    targetTransform.position = originalPosition;
            //    targetTransform.rotation = originalRotation;
            //}
        }

        private bool IsCollidingWithAnything(Transform target)
        {
            Collider[] myColliders = target.GetComponentsInChildren<Collider>();

            foreach (Collider myCollider in myColliders)
            {
                Collider[] overlappingColliders = Physics.OverlapBox(
                    myCollider.bounds.center,
                    myCollider.bounds.extents,
                    myCollider.transform.rotation
                );

                if (overlappingColliders.Length > 1) // >1, ������ ��� ��� ������ ���� �������� � ������
                {
                    return true;
                }
            }
            return false;
        }

        private void SpawnObjectsOnWall(Cell cell, WallHolder wallHolderEnum, string objectName)
        {
            var wallHolder = wallHolderEnum.ToWallObject(cell);
            
            if (wallHolder == null)
            {
                Debug.Log("Wallholder is null");
                return;
            }

            Maze.Wall wall = wallHolder.GetComponentInChildren<Maze.Wall>(true);
            if (wall == null)
            {
                Debug.Log("Wall is none");
                return;
            }

            int wallObjectIndex = GetRandomPrefabIndex(wall.ObjectsOnWalls);
            var wallObject = wall.ObjectsOnWalls[wallObjectIndex].prefab;
            
            if (wallObject == null)
            {
                return;
            }
            
            InstantiateObjectsOnWallsRpc(cell.GetComponent<NetworkObject>(), wallHolderEnum, wallObjectIndex, objectName);
        }

        [Rpc(SendTo.Everyone)]
        private void InstantiateObjectsOnWallsRpc(NetworkObjectReference cellReference, WallHolder wallHolderEnum, int wallObjectIndex, FixedString64Bytes objectName64Bytes)
        {
            cellReference.TryGet(out var cellObj);
            var cell = cellObj.GetComponent<Cell>();
            
            var wallHolder = wallHolderEnum.ToWallObject(cell);
            var wall = wallHolder.GetComponentInChildren<Maze.Wall>(true);
            
            string objectName = objectName64Bytes.ToString();
            Transform wallSlot = wall.transform.Find(objectName);
            var wallObject = wall.ObjectsOnWalls[wallObjectIndex].prefab;
            
            Instantiate(wallObject, wallSlot);
        }

        [Rpc(SendTo.Everyone)]
        private void SpawnBorderWallsRpc(NetworkObjectReference cellReference, int i, int j, int width, int height)
        {
            cellReference.TryGet(out var cellObj);
            SpawnBorderWalls(cellObj.GetComponent<Cell>(), i, j, width, height);
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

        int GetRandomPrefabIndex(List<WeightedPrefab> weightedPrefabs)
        {
            float totalWeight = 0;
            foreach (var wp in weightedPrefabs)
                totalWeight += wp.weight;

            float randomPoint = Random.Range(0, totalWeight);

            foreach (var wp in weightedPrefabs)
            {
                if (randomPoint < wp.weight)
                    return weightedPrefabs.IndexOf(wp);
                randomPoint -= wp.weight;
            }

            return 0;
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
            Vector3 rayStart = bounds.center;
            float rayLength = bounds.extents.y + 2f;

            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, rayLength))
            {
                // �������� �� ������ �� ������ �����
                float bottomOffset = bounds.center.y - bounds.min.y;
                obj.transform.position = hit.point + new Vector3(0, bottomOffset, 0);
            }
            else
            {
                Debug.LogWarning($"������ {obj.name} �� �����������!");
            }
        }
    }
}
