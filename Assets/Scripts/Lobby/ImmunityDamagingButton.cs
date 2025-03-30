using ObjectScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace Lobby
{
    public class ImmunityDamagingButton : NetworkBehaviour, IInteractable
    {
        public void Interact(Transform player)
        {
            player.GetComponent<PlayerHealth>().DamageImmunity(20);
        }
    }
}