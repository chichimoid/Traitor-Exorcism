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
        private readonly NetworkVariable<ulong> _id = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<PlayerState> _state = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<PlayerRole> _role = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [field:SerializeField] public GameObject MeshObject { get; private set; }
        public ulong Id { get => _id.Value; private set => _id.Value = value; }

        public PlayerState State
        {
            get => _state.Value;
            set
            {
                switch (_state.Value)
                {
                    case PlayerState.InLobby: OnPlayerStateFromLobby?.Invoke(); break;
                    case PlayerState.InMaze: OnPlayerStateFromMaze?.Invoke(); break;
                    case PlayerState.InVoting: OnPlayerStateFromVoting?.Invoke(); break;
                }
                
                switch (value)
                {
                    case PlayerState.InLobby: OnPlayerStateToLobby?.Invoke(); break;
                    case PlayerState.InMaze: OnPlayerStateToMaze?.Invoke(); break;
                    case PlayerState.InVoting: OnPlayerStateToVoting?.Invoke(); break;
                }
                _state.Value = value;
            }
        }

        public PlayerRole Role
        {
            get => _role.Value;
            set
            {
                OnPlayerRoleSet?.Invoke(value);
                _role.Value = value;
            }
        }

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
            if (!objReference.TryGet(out var heldObj))
            {
                _heldObjMain = null;
                return;
            }
            _heldObjMain = heldObj.GetComponent<Grabbable>();
        }
        
        [Rpc(SendTo.Everyone)]
        private void SyncSecondHeldObjRpc(NetworkObjectReference objReference)
        {
            if (!objReference.TryGet(out var heldObj))
            {
                _heldObjSecond = null;
                return;
            }
            _heldObjSecond = heldObj.GetComponent<Grabbable>();
        }
        
        private void Start()
        {
            if (!IsOwner) return;

            Id = NetworkManager.Singleton.LocalClientId;
            State = PlayerState.InLobby;
        }
        
        public static NetworkPlayer GetLocalInstance()
        {
            return NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
        }
        
        public static NetworkPlayer GetInstance(ulong clientId)
        {
            return NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<NetworkPlayer>();
        }
        
        public delegate void OnPlayerStateChangedDelegate();
        public event OnPlayerStateChangedDelegate OnPlayerStateToLobby;
        public event OnPlayerStateChangedDelegate OnPlayerStateFromLobby;
        public event OnPlayerStateChangedDelegate OnPlayerStateToMaze;
        public event OnPlayerStateChangedDelegate OnPlayerStateFromMaze;
        public event OnPlayerStateChangedDelegate OnPlayerStateToVoting;
        public event OnPlayerStateChangedDelegate OnPlayerStateFromVoting;

        public delegate void OnPlayerRoleSetDelegate(PlayerRole role);
        public event OnPlayerRoleSetDelegate OnPlayerRoleSet;
    }
}