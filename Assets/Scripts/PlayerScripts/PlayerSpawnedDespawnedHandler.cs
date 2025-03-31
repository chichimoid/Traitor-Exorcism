using NetworkHelperScripts;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Cursor = UnityEngine.Cursor;

namespace PlayerScripts
{
    /// <summary>
    /// Player component script.
    /// Attaches various essential components to the player when one spawns.
    /// Detaches everything, when the player despawns.
    /// Also handles the main camera (PlayerCamera).
    /// </summary>
    public class PlayerSpawnedDespawnedHandler : NetworkBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private Transform cameraPrefab;
        [SerializeField] private Transform uiPrefab;
        
        [Header("Spawn Configuration")]
        [SerializeField] private Transform cameraTargetTransform;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (!IsOwner) return;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            AddPlayerCamera();
            AddPlayerUI();
            transform.AddComponent<PlayerLocker>();
            transform.AddComponent<CrosshairUIManager>();
            
            if (IsServer)
            {
                SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Lobby);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (!IsOwner) return;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            Destroy(PlayerCamera.Instance.gameObject);
            Destroy(SessionManager.Instance.gameObject);
            SceneLoader.Instance.LoadSceneLocal(SceneLoader.Scene.MainMenu);
        }

        private void AddPlayerCamera()
        {
            var tr = Instantiate(cameraPrefab);
            var cameraBinder = tr.GetComponent<PlayerCamera>();
            cameraBinder.Follow(cameraTargetTransform);
            DontDestroyOnLoad(tr.gameObject);
        }

        private void AddPlayerUI()
        {
            var tr = Instantiate(uiPrefab, transform);
            var playerUI = tr.GetComponent<PlayerUI>();
            
            var emoteWheelUI = playerUI.EmoteWheelUI.GetComponent<EmoteWheelUI>();
            var helper = GetComponent<TextHelper>();
            foreach (var emoteButton in emoteWheelUI.EmoteButtons)
            {
                // This is very ugly due to proper emotes being absent.
                // This is only a template for an actual emote system, where emotes are just text signs.
                var text = emoteButton.EmoteName;
                emoteButton.Button.onClick.AddListener(() => helper.TempDisplayText128Chars(text));
            }
            
            PlayerUI.Instance.DisableConditionalUI();
        }
    }
}