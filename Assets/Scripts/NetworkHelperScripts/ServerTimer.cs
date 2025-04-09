using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NetworkHelperScripts
{
    public class ServerTimer : NetworkBehaviour
    {
        [SerializeField] private float timerTick;
        
        private float _timerValue;

        public void StartTimer(int initialValue)
        {
            if (!IsServer) return;
            
            _timerValue = initialValue;
            StartCoroutine(UpdateTimerCoroutine());
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        // Be careful with OnTimeChanged event.
        private IEnumerator UpdateTimerCoroutine()
        {
            while (_timerValue > 0)
            {
                yield return new WaitForSeconds(timerTick);
                
                _timerValue -= timerTick;
                OnTimeChanged?.Invoke(_timerValue);
            }
            
            OnTimeUp?.Invoke();
        }
        
        public delegate void OnTimeChangedDelegate(float newValue);
        public event OnTimeChangedDelegate OnTimeChanged;
        
        public delegate void OnTimeUpDelegate();
        public event OnTimeUpDelegate OnTimeUp;
    }
}