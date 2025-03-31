using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace ObjectScripts
{
    public class PillsUsable : Usable
    {
        private int _count = 3;
        protected override void UseFunctional()
        {
            if (_count == 0)
            {
                Debug.Log("Pills is over...");
                return;
            }
            if (player.GetComponent<PlayerHealth>().Infection == 0)
            {
                Debug.Log("Вы полностью здоровы! Поберегите таблетки.");
                return;
            }
            player.GetComponent<PlayerHealth>().InflictInfection(-10); 
            _count--;
            Debug.Log($"Current infection: {player.GetComponent<PlayerHealth>().Infection}. Осталось использований: {_count}");
        }
    }
}