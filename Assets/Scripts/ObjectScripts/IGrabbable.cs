using UnityEngine;

namespace ObjectScripts
{
    public interface IGrabbable : IInteractable
    {
        public void Drop();
    }
}