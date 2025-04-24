using PlayerScripts;
using UnityEngine;

namespace ObjectScripts
{
    public class FirstaidUsable : Usable
    {
        private int _count = 3;
        public AudioClip emptySound;

        protected override void UseFunctional()
        {
            if(_count == 0)
            {
                Debug.Log("Aptechka is over...");
                return;
            }
            if (Player.GetComponent<PlayerHealth>().Value == Player.GetComponent<PlayerHealth>().MaxValue)
            {
                Debug.Log("You have a full health!");
                return;
            }
            Player.GetComponent<PlayerHealth>().Heal(20);
            _count--;
            if (_count == 0) audioSource.clip = emptySound;
            Debug.Log($"Current health: {Player.GetComponent<PlayerHealth>().Value}. Uses left: {_count}");
        }
    }
}