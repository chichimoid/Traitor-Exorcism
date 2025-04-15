using System;
using System.Collections.Generic;
using System.Collections;
using NetworkHelperScripts;
using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerInfection : NetworkBehaviour
    {
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float proximityIncreaseTick = 0.5f;
        [SerializeField] private float proximityIncreaseValue = 1f;
        [field:SerializeField] public PlayerInfectionCollider PlayerInfectionCollider { get; private set;}
        
        private readonly Stack<Coroutine> _infectingPlayers = new();
        private FixedIntervalFloat _fixedIntervalFloat;
        private PlayerRayCaster _playerRayCaster;
        
        public float Value
        {
            get => _fixedIntervalFloat.Value;
            private set => _fixedIntervalFloat.Value = value;
        }
        
        public float MaxValue => maxValue;
        
        private void Start()
        {
            if (!IsOwner) return;
            
            _playerRayCaster = GetComponent<PlayerRayCaster>();
            
            _fixedIntervalFloat = new FixedIntervalFloat(0, maxValue);
            
            PlayerInfectionCollider.OnPlayerFound += StartInfecting;
            PlayerInfectionCollider.OnPlayerLost += StopInfecting;
        }

        private void OnDisable()
        {
            while (_infectingPlayers.Count > 0)
            {
                StopCoroutine(_infectingPlayers.Pop());
            }
        }

        private void StartInfecting(Collider other)
        {
            var coroutine = StartCoroutine(ProximityIncreaseCoroutine(other)); 
            _infectingPlayers.Push(coroutine);
        }
        
        private void StopInfecting(Collider other)
        {
            if (_infectingPlayers.Count == 0) return;
            StopCoroutine(_infectingPlayers.Pop());
        }
        
        /// <param name="value">positive</param>
        public void Increase(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            Value += value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} infection level changed: {Value}");
            
            CheckInfected();
        }
        
        /// <param name="value">positive</param>
        public void Decrease(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            Value -= value;
            GlobalDebugger.Instance.Log($"Player {NetworkManager.Singleton.LocalClientId} infection level changed: {Value}");
        }
        
        public IEnumerator ProximityIncreaseCoroutine(Collider other)
        {
            while (Value < maxValue)
            {
                if (!_playerRayCaster.PlayerObstacleRayCast(out var hit, other.transform))
                {
                    Increase(proximityIncreaseValue);
                }
                yield return new WaitForSeconds(proximityIncreaseTick);
            }
        }
        
        private void CheckInfected()
        {
            if (_fixedIntervalFloat.Value >= maxValue)
            {
                Value = 0;
                OnPlayerFullyInfected?.Invoke();
            }
        }

        public delegate void OnPlayerFullyInfectedDelegate();
        public event OnPlayerFullyInfectedDelegate OnPlayerFullyInfected;
    }
}
