using UnityEngine;

namespace ObjectScripts
{
    [CreateAssetMenu(fileName = "GrabbableSO", menuName = "Scriptable Objects/GrabbableSO")]
    public class GrabbableSO : ScriptableObject
    {
        public Transform prefab;
        public string objectName;
    }
}
