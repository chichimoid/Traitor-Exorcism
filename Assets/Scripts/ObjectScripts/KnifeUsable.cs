using UnityEngine;
using Unity.Netcode;
using System.Linq;
using PlayerScripts;

namespace ObjectScripts
{
    public class KnifeUsable : Weapon
    {
        private float _range = 1.5f;
        public override float Damage { get; protected set; } = 20f;

        protected override void UseFunctional()
        {
            if (IsOwner)
                TryHitServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryHitServerRpc()
        {
            var hits = Physics.OverlapSphere(transform.position + transform.forward * 0.8f, _range);
            var nearestPlayer = hits
                .Select(h => h.GetComponent<PlayerHealth>()) 
                .Where(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
                .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
                .FirstOrDefault();

            if (nearestPlayer != null)
            {
                nearestPlayer.DamageHealth(Damage);
                Debug.Log($"Nearest player got {Damage} damage, he has {nearestPlayer.Health} health");
            }
        }
    }
}