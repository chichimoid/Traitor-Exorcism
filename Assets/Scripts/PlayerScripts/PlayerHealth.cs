using System;
using System.Collections;
using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float regenTick = 0.5f;
        [SerializeField] private float regenDelay = 3f;
        [SerializeField] private float regenValue = 1f;
        [SerializeField] private float monsterDrainTick = 0.5f;
        [SerializeField] private float monsterDrainValue = 1f;
        
        private FixedIntervalFloat _fixedIntervalFloat;
        private Coroutine _regenAfterDelayCoroutine;
        private Coroutine _monsterHealthDrainCoroutine;

        private bool _isDead = false;

        public float Value
        {
            get => _fixedIntervalFloat.Value;
            private set
            {
                _fixedIntervalFloat.Value = value;
                OnHealthChanged?.Invoke(value);
            }
        }

        public float MaxValue => maxValue;
        
        private void Start()
        {
            if (!IsOwner) return;
            
            _fixedIntervalFloat = new FixedIntervalFloat(maxValue, maxValue);
            GetComponent<PlayerInfection>().OnPlayerFullyInfected += () => OnHealthZero?.Invoke();
        }

        private void OnDisable()
        {
            Value = maxValue;
            
            if (_regenAfterDelayCoroutine != null)
            {
                StopCoroutine(_regenAfterDelayCoroutine);
            }

            if (_monsterHealthDrainCoroutine != null)
            {
                StopCoroutine(_monsterHealthDrainCoroutine);
            }
        }
        
        /// <param name="value">positive</param>
        public void Heal(float value)
        {
            if (!IsOwner || _isDead) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            Value += value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} health changed: {Value}");
        }
        /// <param name="value">positive</param>
        public void Damage(float value)
        {
            if (!IsOwner || _isDead) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            if (_regenAfterDelayCoroutine != null)
            {
                StopCoroutine(_regenAfterDelayCoroutine);
            }
            
            Value -= value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} health changed: {Value}");
            
            CheckDead();
            if (_isDead) return;
            
            if (_monsterHealthDrainCoroutine == null) _regenAfterDelayCoroutine = StartCoroutine(RegenAfterDelayCoroutine());
        }

        public void StartMonsterHealthDrain()
        {
            _monsterHealthDrainCoroutine = StartCoroutine(MonsterHealthDrainCoroutine());
        }
        private IEnumerator MonsterHealthDrainCoroutine()
        {
            while (Value > 0)
            {
                Damage(monsterDrainValue);
                yield return new WaitForSeconds(monsterDrainTick);
            }
        }
        
        private IEnumerator RegenAfterDelayCoroutine()
        {
            yield return new WaitForSeconds(regenDelay);

            while (Value < maxValue)
            {
                Heal(regenValue);
                yield return new WaitForSeconds(regenTick);
            }
        }
        
        private void CheckDead()
        {
            if (Value == 0f)
            {
                _isDead = true;
                Value = maxValue;
                OnHealthZero?.Invoke();
            }
        }

        public delegate void OnHealthChangedDelegate(float value);
        public event OnHealthChangedDelegate OnHealthChanged;
        
        public delegate void OnHealthZeroDelegate();
        public event OnHealthZeroDelegate OnHealthZero;

    }
}
