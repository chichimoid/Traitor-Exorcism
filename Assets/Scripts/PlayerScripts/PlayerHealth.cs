using System;
using System.Collections;
using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerScripts
{
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float regenTick = 0.5f;
        [SerializeField] private float regenDelay = 3f;
        [SerializeField] private float regenValue = 1f;
        
        private FixedIntervalFloat _fixedIntervalFloat;
        private Coroutine _regenAfterDelayCoroutine;

        public float Value
        {
            get => _fixedIntervalFloat.Value;
            private set => _fixedIntervalFloat.Value = value;
        }

        public float MaxValue => maxValue;
        
        private void Start()
        {
            if (!IsOwner) return;
            
            _fixedIntervalFloat = new FixedIntervalFloat(maxValue, maxValue);
        }
        /// <param name="value">positive</param>
        public void Regen(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            Value += value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} health changed: {Value}");
        }
        /// <param name="value">positive</param>
        public void Damage(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            if (_regenAfterDelayCoroutine != null)
            {
                StopCoroutine(_regenAfterDelayCoroutine);
            }
            
            Value -= value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} health changed: {Value}");
            CheckDead();
            
            _regenAfterDelayCoroutine = StartCoroutine(RegenAfterDelayCoroutine());
        }
        
        private IEnumerator RegenAfterDelayCoroutine()
        {
            yield return new WaitForSeconds(regenDelay);

            while (Value < maxValue)
            {
                Regen(regenValue);
                yield return new WaitForSeconds(regenTick);
            }
        }
        
        private void CheckDead()
        {
            if (Value == 0f)
            {
                OnPlayerDied?.Invoke();
            }
        }

        public delegate void OnPlayerDiedDelegate();

        public event OnPlayerDiedDelegate OnPlayerDied;

    }
}
