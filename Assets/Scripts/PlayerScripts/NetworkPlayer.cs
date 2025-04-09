using JetBrains.Annotations;
using ObjectScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerScripts
{
    public enum PlayerState
    {
        InLobby,
        InMaze,
        InVoting,
    }

    public enum PlayerRole
    {
        None,
        Survivor,
        Monster
    }
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private GameObject meshObject;
        
        private readonly NetworkVariable<ulong> _id = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<PlayerState> _state = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<PlayerRole> _role = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public GameObject MeshObject => meshObject;
        public ulong Id { get => _id.Value; private set => _id.Value = value; }

        public PlayerState State
        {
            get => _state.Value;
            set
            {
                switch (_state.Value)
                {
                    case PlayerState.InLobby: OnPlayerStateFromInLobby?.Invoke(); break;
                    case PlayerState.InMaze: OnPlayerStateFromInMaze?.Invoke(); break;
                    case PlayerState.InVoting: OnPlayerStateFromInVoting?.Invoke(); break;
                }
                
                switch (value)
                {
                    case PlayerState.InLobby: OnPlayerStateInLobby?.Invoke(); break;
                    case PlayerState.InMaze: OnPlayerStateInMaze?.Invoke(); break;
                    case PlayerState.InVoting: OnPlayerStateInVoting?.Invoke(); break;
                }
                _state.Value = value;
            }
        }

        public PlayerRole Role { get => _role.Value; set => _role.Value = value; }

        private Grabbable _heldObjMain;
        private Grabbable _heldObjSecond;

        [CanBeNull]
        public Grabbable HeldObjMain
        {
            get => _heldObjMain;
            set => SyncMainHeldObjRpc(value?.GetComponent<NetworkObject>());
        }
        public Grabbable HeldObjSecond
        {
            get => _heldObjSecond;
            set => SyncSecondHeldObjRpc(value?.GetComponent<NetworkObject>());
        }

        [Rpc(SendTo.Everyone)]
        private void SyncMainHeldObjRpc(NetworkObjectReference objReference)
        {
            objReference.TryGet(out var heldObj);
            _heldObjMain = heldObj.GetComponent<Grabbable>();
        }
        
        [Rpc(SendTo.Everyone)]
        private void SyncSecondHeldObjRpc(NetworkObjectReference objReference)
        {
            objReference.TryGet(out var heldObj);
            _heldObjSecond = heldObj.GetComponent<Grabbable>();
        }


        private void Start()
        {
            if (!IsOwner) return;

            Id = NetworkManager.Singleton.LocalClientId;
            State = PlayerState.InLobby;
        }
        
        public delegate void OnPlayerStateChangedDelegate();
        public event OnPlayerStateChangedDelegate OnPlayerStateInLobby;
        public event OnPlayerStateChangedDelegate OnPlayerStateFromInLobby;
        public event OnPlayerStateChangedDelegate OnPlayerStateInMaze;
        public event OnPlayerStateChangedDelegate OnPlayerStateFromInMaze;
        public event OnPlayerStateChangedDelegate OnPlayerStateInVoting;
        public event OnPlayerStateChangedDelegate OnPlayerStateFromInVoting;
    }
}