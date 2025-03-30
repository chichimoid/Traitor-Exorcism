using System.Collections;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

public class PlayerStamina : NetworkBehaviour
{
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float sprintStaminaWaste = 0.5f;
        [SerializeField] private float regenTick = 0.1f;
        [SerializeField] private float regenDelay = 2f;
        [SerializeField] private float regenValue = 2f;
        
        private FixedIntervalFloat _stamina;
        private Coroutine _regenAfterDelayCoroutine;
        
        public float Stamina {
            get => _stamina.Value; 
            set => _stamina.Value = value;
        }

        private void Start()
        {
            if (!IsOwner) return;
            _stamina = new FixedIntervalFloat(maxStamina, maxStamina);
            
            var playerMovement = GetComponent<PlayerMovement>();
            playerMovement.OnPlayerIsRunning += () => WasteStamina(sprintStaminaWaste);
        }

        public void WasteStamina(float value)
        {
            if (!IsOwner) return;
            
            if (_regenAfterDelayCoroutine != null)
            {
                StopCoroutine(_regenAfterDelayCoroutine);
            }
            
            Stamina -= value;
            Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} stamina changed: {Stamina}");
            
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
                Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} stamina changed: {Stamina}");
                yield return new WaitForSeconds(regenTick);
            }
        }
        public delegate void OnPlayedTiredDelegate();
        public event OnPlayedTiredDelegate OnPlayedTired;
        
        public delegate void OnPlayerUnTiredDelegate();
        public event OnPlayerUnTiredDelegate OnPlayerUnTired;
    }