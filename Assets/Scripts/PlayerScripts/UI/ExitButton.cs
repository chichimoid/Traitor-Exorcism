using NetworkHelperScripts;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerScripts
{
    public class ExitButton : MonoBehaviour
    {
        private Button _button;

        void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(SessionManager.Instance.LeaveSession);
        }
    }
}