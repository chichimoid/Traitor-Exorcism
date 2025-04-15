using UnityEngine;
using System.Collections.Generic;

namespace Maze
{
    [System.Serializable]
    public class WeightedPrefab
    {
        public GameObject prefab;
        public float weight = 1f;
    }
    
    public enum CellObjectList
    {
        Walls,
        Items
    }
    public static class CellObjectListExtensions
    {
        public static List<WeightedPrefab> ToWeightedPrefabList(this CellObjectList cellObjectList, Cell cell)
        {
            return cellObjectList switch
            {
                CellObjectList.Walls => cell.walls,
                CellObjectList.Items => cell.items,
                _ => null
            };
        }
    }
    
    public enum WallHolder
    {
        Left,
        Bottom
    }
    public static class WallHolderExtensions
    {
        public static GameObject ToWallObject(this WallHolder wallHolder, Cell cell)
        {
            return wallHolder switch
            {
                WallHolder.Left => cell.wallLeft,
                WallHolder.Bottom => cell.wallBottom,
                _ => null
            };
        }
    }
    
    public class Cell : MonoBehaviour
    {
        public GameObject wallLeft;
        public GameObject wallBottom;
        public GameObject doorLeft;
        public GameObject doorBottom;
        public GameObject floor;
        public GameObject lever;

        public List<WeightedPrefab> walls;
        public List<WeightedPrefab> items;
    }
}