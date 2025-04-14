using System;
using System.Collections;
using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerScripts
{
    public class MonsterBar : NetworkBehaviour
    {
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float fillingTick = 1f;
        [SerializeField] private float fillingValue = 1f;
        
        private FixedIntervalFloat _fixedIntervalFloat;
        
        public float Value {
            get => _fixedIntervalFloat.Value; 
            set => _fixedIntervalFloat.Value = value;
        }
        public float MaxValue => maxValue;
        private void Start()
        {
            if (!IsOwner) return;
            
            _fixedIntervalFloat = new FixedIntervalFloat(0, maxValue);
            
            EnabledServerRpc(false);
        }

        [Rpc(SendTo.Everyone)]
        private void EnabledServerRpc(bool value)
        {
            enabled = value;
        }

        public void StartFilling()
        {
            StartCoroutine(IncreaseCoroutine());
        }
        
        /// <param name="value">positive</param>
        public void Increase(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            Value += value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} Monster Bar changed: {Value}");

            CheckMonster();
        }
        
        /// <param name="value">positive</param>
        public void Decrease(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            Value -= value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} Monster Bar changed: {Value}");
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        // Be careful with fillingTick.
        private IEnumerator IncreaseCoroutine()
        {
            while (Value < maxValue)
            {
                Increase(fillingValue);
                yield return new WaitForSeconds(fillingTick);
            }
        }
        
        private void CheckMonster()
        {
            if (Value >= maxValue)
            {
                OnMonsterTurnedServerRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void OnMonsterTurnedServerRpc()
        {
            OnMonsterTurned?.Invoke();
        }

        public delegate void OnMonsterTurnedDelegate();
        public event OnMonsterTurnedDelegate OnMonsterTurned;
    }
}
