using NUnit.Framework;
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