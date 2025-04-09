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
            if (Player.GetComponent<PlayerHealth>().Infection == 0)
            {
                Debug.Log("�� ��������� �������! ���������� ��������.");
                return;
            }
            Player.GetComponent<PlayerHealth>().InflictInfection(-10); 
            _count--;
            Debug.Log($"Current infection: {Player.GetComponent<PlayerHealth>().Infection}. �������� �������������: {_count}");
        }
    }
}