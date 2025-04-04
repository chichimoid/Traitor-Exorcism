using Unity.Netcode;

namespace ObjectScripts
{
    public abstract class Usable : Grabbable, IUsable
    {
        public void Use()
        {
            UseRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void UseRpc()
        {
            UseFunctional();
        }

        protected abstract void UseFunctional();
    }
}