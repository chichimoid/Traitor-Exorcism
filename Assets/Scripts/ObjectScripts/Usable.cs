using Unity.Netcode;
using UnityEngine;

namespace ObjectScripts
{
    public abstract class Usable : Grabbable, IUsable
    {
        public AudioSource audioSource;
        public void Use()
        {
            UseRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void UseRpc()
        {
            UseSound();
            UseFunctional();
        }

        protected abstract void UseFunctional();
        protected void UseSound()
        {
            if (audioSource)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }
        }

    }
}