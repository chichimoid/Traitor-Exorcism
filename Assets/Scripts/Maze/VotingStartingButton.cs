using ObjectScripts;
using Unity.Netcode;
using UnityEngine;

namespace Maze
{
    public class VotingStartingButton : NetworkBehaviour, IInteractable
    {
        public void Interact(Transform player)
        {
            GameManager.Instance.StartVoting();
        }
    }
}