using UnityEngine;
using System.Collections.Generic;
using ObjectScripts;

// Commented out since the list SO pretty much never needs to be replicated
//[CreateAssetMenu(fileName = "GrabbableObjectSOList", menuName = "Scriptable Objects/GrabbableObjectSOList")]
public class SerializableSOList : ScriptableObject
{
    public List<GrabbableSO> grabbables;
}