using UnityEngine;

namespace Maze
{
    [CreateAssetMenu(fileName = "CellSO", menuName = "Scriptable Objects/CellSO")]
    public class CellSO : ScriptableObject
    {
        public Transform prefab;
        public string objectName;
    }

}