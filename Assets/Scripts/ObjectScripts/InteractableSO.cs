using UnityEngine;

namespace ObjectScripts
{
    [CreateAssetMenu(fileName = "InteractableSO", menuName = "Scriptable Objects/InteractableSO")]
    public class InteractableSO : ScriptableObject
    {
        public Transform prefab;
        public string objectName;
    }
}