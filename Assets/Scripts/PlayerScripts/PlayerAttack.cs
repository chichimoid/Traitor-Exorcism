using System.Collections;
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
        [SerializeField] private float meleeAttackRange;
        [SerializeField] private float bareHandDamage;
        [SerializeField] private float attackCooldownSeconds;
        
        private NetworkPlayer _networkPlayer;
        private PlayerInputActions _inputActions;
        private InputAction _attack;
        private PlayerRayCaster _rayCaster;
        
        private bool _isOnCooldown = false;

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
            
            if (_rayCaster.ViewPointRayCast(out var hit,  meleeAttackRange)) 
            {
                if (hit.collider.TryGetComponent(out NetworkPlayer otherPlayer) && otherPlayer.State == PlayerState.InMaze && !_isOnCooldown)
                {
                    var heldObj = _networkPlayer.HeldObjMain;

                    var attackDamage = (heldObj as Weapon)?.Damage ?? bareHandDamage;
                    
                    StartCoroutine(AttackCooldownCoroutine());

                    AttackTargetRpc(otherPlayer.gameObject, attackDamage, RpcTarget.Single(otherPlayer.Id, RpcTargetUse.Temp));
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void AttackTargetRpc(NetworkObjectReference attackedPlayerReference, float damage, RpcParams rpcParams)
        {
            attackedPlayerReference.TryGet(out var attackedPlayer);
            var attackedPlayerHealth = attackedPlayer.GetComponent<PlayerHealth>();
            attackedPlayerHealth.Damage(damage);
        }

        private IEnumerator AttackCooldownCoroutine()
        {
            _isOnCooldown = true;
            yield return new WaitForSeconds(attackCooldownSeconds);
            _isOnCooldown = false;
        }
    }
}
