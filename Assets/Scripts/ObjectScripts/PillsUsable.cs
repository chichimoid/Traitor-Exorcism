using PlayerScripts;
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
            
            if (Player.GetComponent<NetworkPlayer>().Role == PlayerRole.Monster)
            {
                Player.GetComponent<MonsterBar>().Increase(10);
            }
            else
            {
                if (Player.GetComponent<PlayerInfection>().Value == 0)
                {
                    Debug.Log("�� ��������� �������! ���������� ��������.");
                    return;
                }

                Player.GetComponent<PlayerInfection>().Decrease(10);
            }
            
            _count--;
            Debug.Log($"Current infection: {Player.GetComponent<PlayerInfection>().Value}. �������� �������������: {_count}");
        }
    }
}