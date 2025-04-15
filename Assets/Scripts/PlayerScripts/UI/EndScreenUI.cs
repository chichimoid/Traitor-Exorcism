using TMPro;
using UnityEngine;

namespace PlayerScripts.UI
{
    public class EndScreenUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject uiObject;
        [SerializeField] private TMP_Text resultTextField;

        private string _roleDependentVictoryText;
        private string _roleDependentLossText;
        
        private void Start()
        {
            uiObject.SetActive(false);
        }
        
        public void Show(bool hasWon, ulong votedPlayerId)
        {
            var role = NetworkPlayer.GetLocalInstance().Role;
            
            if (role == PlayerRole.Survivor)
            {
                _roleDependentVictoryText = $"Player {votedPlayerId} was indeed the monster!\nYou won!";
                _roleDependentLossText = $"Player {votedPlayerId} was not the monster...\nPlayer {GameManager.GetMonsterId()} has won!";
            } else
            {
                _roleDependentVictoryText = "They did not vote you out!\nYou won!";
                _roleDependentLossText = "They voted you out...\nYou lost!";
            }
            
            uiObject.SetActive(true);
            
            resultTextField.text = hasWon ? _roleDependentVictoryText : _roleDependentLossText;
        }
    }
}