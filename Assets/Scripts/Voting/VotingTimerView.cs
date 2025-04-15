using NetworkHelperScripts;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Voting
{
    public class VotingTimerView : NetworkBehaviour
    {
        [SerializeField] private ServerTimer serverTimer;
        [SerializeField] private TMP_Text timerTextField;

        private void Start()
        {
            serverTimer.OnTimeChanged += UpdateServerTimerTextRpc;
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateServerTimerTextRpc(float timerValue)
        {
            timerTextField.text = timerValue.ToString("0.0");
        }
    }
}