using MainMenu;
using TMPro;
using UnityEngine;

namespace Lobby
{
    public class LobbyCode : MonoBehaviour
    {
        private TextMeshPro _text;
        private void Start()
        {
            _text = GetComponent<TextMeshPro>();
            _text.text = ConnectionManager.Instance.LobbyCode;
        }
    }
}
