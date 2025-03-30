using Maze;
using ObjectScripts;
using Unity.Netcode;
using UnityEngine;

namespace Lobby
{
    public class GameStartingButton : NetworkBehaviour, IInteractable
    {
        [SerializeField] private string clientAttemptMsg;
        private TextHelper _textHelper;
        
        private void Awake()
        {
            _textHelper = GetComponent<TextHelper>();
        }
        
        public void Interact(Transform player)
        {
            if (!IsServer)
            {
                _textHelper.TempDisplayText32Chars(clientAttemptMsg);
                return;
            }
            
            GameManager.Instance.StartGame();
        }
    }
}