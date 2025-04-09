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
            if(Player.GetComponent<PlayerHealth>().Health == Player.GetComponent<PlayerHealth>().MaxHealth)
            {
                Debug.Log("You have a full health! ������� ���� �������!");
                return;
            }
            Player.GetComponent<PlayerHealth>().RegenHealth(20);
            _count--;
            Debug.Log($"Current health: {Player.GetComponent<PlayerHealth>().Health}. �������� �������������: {_count}");
        }
    }
}