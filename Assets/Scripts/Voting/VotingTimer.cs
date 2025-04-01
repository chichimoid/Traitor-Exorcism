using System.Collections;
using Maze;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Voting
{
    public class VotingTimer : NetworkBehaviour
    {
        [SerializeField] private float timerTick = 0.1f;
        [SerializeField] private TMP_Text timerTextField;
        [SerializeField] private int timerInitialValueSeconds = 30;
        
        private float _timerValue;
        
        public static VotingTimer Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            
            _timerValue = timerInitialValueSeconds;
        }

        public void StartTimer()
        {
            OnTimeUp += GameManager.Instance.Conclude;
            
            StartTimerRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void StartTimerRpc()
        {
            StartCoroutine(UpdateTimerCoroutine());
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        // Performance analysis is silly and thinks this coroutine will spam OnTimeUp event, which is clearly not the case.
        private IEnumerator UpdateTimerCoroutine()
        {
            while (_timerValue > 0)
            {
                yield return new WaitForSeconds(timerTick);
            
                _timerValue -= timerTick;
                timerTextField.text = _timerValue.ToString("0.0");
            }
            
            OnTimeUp?.Invoke();
        }
        
        public delegate void OnTimeUpDelegate();
        public event OnTimeUpDelegate OnTimeUp;
    }
}