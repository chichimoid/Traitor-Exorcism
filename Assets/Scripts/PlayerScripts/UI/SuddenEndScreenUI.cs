using TMPro;
using UnityEngine;

namespace PlayerScripts.UI
{
    public class SuddenEndScreenUI : MonoBehaviour
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
        
        public void Show(bool hasWon, ulong monsterId)
        {
            if (monsterId == ulong.MaxValue)
            {
                uiObject.SetActive(true);
                resultTextField.text = "Everyone is dead...\n What are you even doing?";
                return;
            }
            
            var role = NetworkPlayer.GetLocalInstance().Role;
            
            if (role == PlayerRole.Survivor)
            {
                _roleDependentVictoryText = $"The monster has died... You won!";
                _roleDependentLossText = $"Player {monsterId} killed everyone...\nThe monster won!";
            } else
            {
                _roleDependentVictoryText = "You killed everyone!\nYou won!";
                _roleDependentLossText = "Did you really just die...\nYou lost!";
            }
            
            uiObject.SetActive(true);
            
            resultTextField.text = hasWon ? _roleDependentVictoryText : _roleDependentLossText;
        }
    }
}