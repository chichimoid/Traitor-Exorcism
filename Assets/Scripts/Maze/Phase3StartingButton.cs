using ObjectScripts;
using Unity.Netcode;
using UnityEngine;

namespace Maze
{
    public class Phase3StartingButton : NetworkBehaviour, IInteractable
    {
        public void Interact(Transform player)
        {
            //GameManager.Instance.StartPhase3();
        }
    }
}