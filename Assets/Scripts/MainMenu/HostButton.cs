using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class HostButton : MonoBehaviour
    {
        private Button _hostButton;
        void Start()
        {
            // Waiting for the EventBus implementation to create a proper await thing.
            _hostButton = GetComponent<Button>();

            _hostButton.onClick.AddListener(ConnectionManager.Instance.CreateRelay);
        }
    }
}
