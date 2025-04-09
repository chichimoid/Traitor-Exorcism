using Maze;
using ObjectScripts;
using Unity.Netcode;
using UnityEngine;

namespace Lobby
{
    public class GameStartingButton : NetworkBehaviour, IInteractable
    {
        [SerializeField] private string clientAttemptMsg;
        private TextDisplayer _textDisplayer;
        
        private void Awake()
        {
            _textDisplayer = GetComponent<TextDisplayer>();
        }
        
        public void Interact(Transform player)
        {
            if (!IsServer)
            {
                _textDisplayer.TempDisplayText32Chars(clientAttemptMsg);
                return;
            }
            
            GameManager.Instance.StartGame();
        }
    }
}