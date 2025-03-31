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
            if (!IsHost)
            {
                _textHelper.TempDisplayText128Chars(clientAttemptMsg);
                return;
            }
            SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Maze);
        }
    }
}