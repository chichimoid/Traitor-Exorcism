using Unity.Netcode;

namespace ObjectScripts
{
    public abstract class Usable : Grabbable, IUsable
    {
        public void Use()
        {
            UseServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void UseServerRpc()
        {
            UseClientRpc();
        }

        [ClientRpc]
        private void UseClientRpc()
        {
            UseFunctional();
        }

        protected abstract void UseFunctional();
    }
}