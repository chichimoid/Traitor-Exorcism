using System.Collections;
using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxImmunity = 100f;
        [SerializeField] private float regenTick = 0.5f;
        [SerializeField] private float regenDelay = 3f;
        [SerializeField] private float regenValue = 1f;
        
        private FixedIntervalFloat _health;
        private FixedIntervalFloat _immunity;
        private FixedIntervalFloat _infection;
        private Coroutine _regenAfterDelayCoroutine;
        
        public float Health {
            get => _health.Value; 
            set => _health.Value = value;
        }
        public float Immunity {
            get => _immunity.Value; 
            set => _immunity.Value = value;
        }

        public float Infection
        {
            get => _infection.Value;
            set => _infection.Value = value;
        }

        private void Start()
        {
            if (!IsOwner) return;
            _health = new FixedIntervalFloat(maxHealth, maxHealth);
            _immunity = new FixedIntervalFloat(maxImmunity, maxImmunity);
            _infection = new FixedIntervalFloat(0, maxImmunity);
        }

        public void DamageHealth(float value)
        {
            if (!IsOwner) return;
            
            if (_regenAfterDelayCoroutine != null)
            {
                StopCoroutine(_regenAfterDelayCoroutine);
            }
            
            Health -= value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} health changed: {Health}");
            CheckDead();
            
            _regenAfterDelayCoroutine = StartCoroutine(RegenAfterDelayCoroutine());
        }
        
        public void DamageImmunity(float value)
        {
            if (!IsOwner) return;
            
            Immunity -= value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} immuniyu changed: {Health}");
            CheckInfected();
        }

        public void InflictInfection(float value)
        {
            if (!IsOwner) return;
            
            Infection += value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} infection level changed: {Infection}");
            CheckInfected();
        }
        
        private IEnumerator RegenAfterDelayCoroutine()
        {
            yield return new WaitForSeconds(regenDelay);

            while (Health < 100f)
            {
                Health += regenValue;
                GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} health changed: {Health}");
                yield return new WaitForSeconds(regenTick);
            }
        }
        
        private void CheckDead()
        {
            if (Health == 0f)
            {
                Die();
            }
        }
        
        private void CheckInfected()
        {
            if (_infection.Value >= _immunity.Value)
            {
                Die();
            }
        }

        private void Die()
        {
            GlobalDebugger.Instance.Log("Player is dead");
        }
    }
}
