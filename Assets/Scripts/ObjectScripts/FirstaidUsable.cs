using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace ObjectScripts
{
    public class FirstaidUsable : Usable
    {
        private int _count = 3;
        protected override void UseFunctional()
        {
            if(_count == 0)
            {
                Debug.Log("Aptechka is over...");
                return;
            }
            if(player.GetComponent<PlayerHealth>().Health == player.GetComponent<PlayerHealth>().MaxHealth)
            {
                Debug.Log("You have a full health! —береги свою аптечку!");
                return;
            }
            player.GetComponent<PlayerHealth>().DamageHealth(-20);
            _count--;
            Debug.Log($"Current health: {player.GetComponent<PlayerHealth>().Health}. ќсталось использований: {_count}");
        }
    }
}