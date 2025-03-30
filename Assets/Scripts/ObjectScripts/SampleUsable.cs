using UnityEngine;

namespace ObjectScripts
{
    public class SampleUsable : Usable
    {
        protected override void UseFunctional()
        {
            Debug.Log("Used");
        }
    }
}