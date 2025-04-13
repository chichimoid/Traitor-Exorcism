using System;
using System.Collections;
using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerScripts
{
    public class PlayerInfection : NetworkBehaviour
    {
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float proximityIncreaseTick = 0.5f;
        [SerializeField] private float proximityIncreaseValue = 1f;
        
        private FixedIntervalFloat _fixedIntervalFloat;
        private Coroutine _regenIncreaseCoroutine;
        
        public float Value
        {
            get => _fixedIntervalFloat.Value;
            private set => _fixedIntervalFloat.Value = value;
        }
        
        public float MaxValue => maxValue;
        
        private void Start()
        {
            if (!IsOwner) return;
            
            _fixedIntervalFloat = new FixedIntervalFloat(0, maxValue);
        }
        
        /// <param name="value">positive</param>
        public void Increase(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            Value += value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} infection level changed: {Value}");
        }
        
        /// <param name="value">positive</param>
        public void Decrease(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            if (_regenIncreaseCoroutine != null)
            {
                StopCoroutine(_regenIncreaseCoroutine);
            }
            
            Value -= value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} infection level changed: {Value}");
            
            CheckInfected();
        }
        
        private IEnumerator ProximityIncreaseCoroutine()
        {
            while (Value < maxValue)
            {
                Increase(proximityIncreaseValue);
                yield return new WaitForSeconds(proximityIncreaseTick);
            }
        }
        
        private void CheckInfected()
        {
            if (_fixedIntervalFloat.Value >= maxValue)
            {
                OnPlayerFullyInfected?.Invoke();
            }
        }

        public delegate void OnPlayerFullyInfectedDelegate();
        public event OnPlayerFullyInfectedDelegate OnPlayerFullyInfected;
    }
}
