using ObjectScripts;
using Unity.Netcode;
using UnityEngine;

namespace Maze
{
    public class Phase2StartingButton : NetworkBehaviour, IInteractable
    {
        public void Interact(Transform player)
        {
            //GameManager.Instance.StartPhase2();
        }
    }
}