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
    public class CellView : MonoBehaviour
    {
        [field:SerializeField] public GameObject WallLeft { get; private set; }
        [field:SerializeField] public GameObject WallBottom { get; private set; }
        [field:SerializeField] public Transform DoorLeft { get; private set; }
        [field:SerializeField] public Transform DoorBottom { get; private set; }
        [field:SerializeField] public GameObject Floor { get; private set; }
        [field:SerializeField] public Transform Lever { get; private set; }
        [field:SerializeField] public List<WeightedPrefab> Walls { get; private set; }
    }
}