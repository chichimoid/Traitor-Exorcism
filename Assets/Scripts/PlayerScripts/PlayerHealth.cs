using System.Collections;
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
        
        public bool IsMonster { get; private set; }
        
        public float MaxHealth
        {
            get => maxHealth; 
        }
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
            Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} health changed: {Health}");
            CheckDead();
            
            _regenAfterDelayCoroutine = StartCoroutine(RegenAfterDelayCoroutine());
        }
        
        public void DamageImmunity(float value)
        {
            if (!IsOwner) return;
            
            Immunity -= value;
            Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} immunity changed: {Immunity}");
            CheckInfected();
        }

        public void InflictInfection(float value)
        {
            if (!IsOwner) return;
            
            Infection += value;
            Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} infection level changed: {Infection}");
            CheckInfected();
        }

        private void CheckDead()
        {
            if (Health == 0f)
            {
                Die();
            }
        }
        private IEnumerator RegenAfterDelayCoroutine()
        {
            yield return new WaitForSeconds(regenDelay);

            while (Health < 100f)
            {
                Health += regenValue;
                Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} health changed: {Health}");
                yield return new WaitForSeconds(regenTick);
            }
        }

        private void CheckInfected()
        {
            if (_infection.Value >= _immunity.Value)
            {
                TurnToMonster();
            }
        }
        
        private void TurnToMonster()
        {
            Debug.Log("Player is a monster now");
            IsMonster = true;
        }

        private void Die()
        {
            Debug.Log("Player is dead");
        }
    }
}
