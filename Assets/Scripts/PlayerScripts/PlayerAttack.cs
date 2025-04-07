using ObjectScripts;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerScripts
{
    public class PlayerAttack : NetworkBehaviour
    {
        [Header("Configure")]
        [SerializeField] private float meleeAttackRange = 3f;
        [SerializeField] private float bareHandDamage = 10f;
        
        private NetworkPlayer _networkPlayer;
        private PlayerInputActions _inputActions;
        private InputAction _attack;
        private PlayerRayCaster _rayCaster;

        private void Start()
        {
            if (!IsOwner) return;

            _networkPlayer = GetComponent<NetworkPlayer>();
            _inputActions = GetComponent<InputActionsContainer>().InputActions;
            _rayCaster = GetComponent<PlayerRayCaster>();

            _attack = _inputActions.Player.Attack;
            
            OnEnable();
        }
        private void OnEnable()
        {
            if (!IsOwner || !didStart) return;
            
            _attack.performed += Attack;
        }

        private void OnDisable()
        {
            if (!IsOwner) return;
            
            _attack.performed -= Attack;
        }
    
        private void Attack(InputAction.CallbackContext context)
        {
            Debug.Log("Attack");
            
            if (_rayCaster.ViewPointRayCast(out RaycastHit hit,  meleeAttackRange)) 
            {
                if (hit.collider.TryGetComponent(out NetworkPlayer otherPlayer) && otherPlayer.State == PlayerState.InMaze)
                {
                    var heldObj = _networkPlayer.HeldObjMain;

                    var attackDamage = (heldObj as Weapon)?.Damage ?? bareHandDamage;

                    AttackRpc(otherPlayer.gameObject, attackDamage, RpcTarget.Single(otherPlayer.Id, RpcTargetUse.Temp));
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void AttackRpc(NetworkObjectReference attackedPlayerReference, float damage, RpcParams rpcParams)
        {
            attackedPlayerReference.TryGet(out var attackedPlayer);
            var attackedNetworkPlayer = attackedPlayer.GetComponent<NetworkPlayer>();
            var attackedPlayerHealth = attackedPlayer.GetComponent<PlayerHealth>();
            
            if (NetworkManager.Singleton.LocalClientId == attackedNetworkPlayer.Id) attackedPlayerHealth.DamageHealth(damage);
        }
    }
}
