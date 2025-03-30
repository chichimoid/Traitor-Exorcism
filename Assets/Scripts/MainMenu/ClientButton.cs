using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class ClientButton : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputJoinCode;
    
        private Button _clientButton;
        void Start()
        {
            // Waiting for the EventBus implementation to create a proper await thing.
            _clientButton = GetComponent<Button>();

            _clientButton.onClick.AddListener(() => ConnectionManager.Instance.JoinRelay(inputJoinCode.text));
        }
    }
}