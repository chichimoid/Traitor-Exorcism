using PlayerScripts;
using UnityEngine;

namespace ObjectScripts
{
    public class PillsUsable : Usable
    {
        private int _count = 3;
        public AudioClip emptySound;
        public AudioClip lastPillSound;

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
                Player.GetComponent<PlayerInfection>().Decrease(10);
            }
            
            _count--;
            if (_count == 1) audioSource.clip = lastPillSound;
            if (_count == 0) audioSource.clip = emptySound;
            Debug.Log($"Current infection: {Player.GetComponent<PlayerInfection>().Value}. Pills left: {_count}");
        }
    }
}