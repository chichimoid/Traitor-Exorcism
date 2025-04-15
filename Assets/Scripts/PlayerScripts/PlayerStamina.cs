using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerStamina : NetworkBehaviour
    {
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float sprintWasteValue = 0.5f;
        [SerializeField] private float regenTick = 0.1f;
        [SerializeField] private float regenDelay = 2f;
        [SerializeField] private float regenValue = 2f;
        
        private FixedIntervalFloat _stamina;
        private Coroutine _regenAfterDelayCoroutine;
        
        public float Stamina {
            get => _stamina.Value; 
            private set => _stamina.Value = value;
        }

        private void Start()
        {
            if (!IsOwner) return;
            
            _stamina = new FixedIntervalFloat(maxValue, maxValue);
            
            var playerMovement = GetComponent<PlayerMovement>();
            playerMovement.OnPlayerIsRunning += () => Decrease(sprintWasteValue);
        }
        
        /// <param name="value">Positive, use Increase() for negative.</param>
        public void Decrease(float value)
        {
            if (!IsOwner) return;
            
            if (value <= 0) throw new ArgumentException("<value> must be positive.");
            
            if (_regenAfterDelayCoroutine != null)
            {
                StopCoroutine(_regenAfterDelayCoroutine);
            }
            
            Stamina -= value;
            //Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} stamina changed: {Stamina}");
            
            CheckTired();
            
            _regenAfterDelayCoroutine = StartCoroutine(RegenAfterDelayCoroutine());
        }

        private void CheckTired()
        {
            if (Stamina == 0f)
            {
                OnPlayedTired?.Invoke();
            }
        }
        private IEnumerator RegenAfterDelayCoroutine()
        {
            yield return new WaitForSeconds(regenDelay);
            
            OnPlayerUnTired?.Invoke();

            while (Stamina < 100f)
            {
                Stamina += regenValue;
                //Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} stamina changed: {Stamina}");
                yield return new WaitForSeconds(regenTick);
            }
        }
        public delegate void OnPlayedTiredDelegate();
        public event OnPlayedTiredDelegate OnPlayedTired;
        
        public delegate void OnPlayerUnTiredDelegate();
        public event OnPlayerUnTiredDelegate OnPlayerUnTired;
    }
}