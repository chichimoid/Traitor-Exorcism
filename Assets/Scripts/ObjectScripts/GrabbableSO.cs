using UnityEngine;

namespace ObjectScripts
{
    [CreateAssetMenu(fileName = "GrabbableObjectSO", menuName = "Scriptable Objects/GrabbableObjectSO")]
    public class GrabbableSO : ScriptableObject
    {
        public Transform prefab;
        public string objectName;
    }
}
