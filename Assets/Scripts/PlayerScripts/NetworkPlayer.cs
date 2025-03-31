using JetBrains.Annotations;
using ObjectScripts;
using Unity.Netcode;
using UnityEngine;

namespace PlayerScripts
{
    public enum PlayerState
    {
        Survivor,
        Monster,
        Spectator
    }
    public class NetworkPlayer : NetworkBehaviour
    {
        private NetworkVariable<ulong> _playerId = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<PlayerState> _playerState = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public ulong PlayerId { get =>  _playerId.Value; private set => _playerId.Value = value; }
        public PlayerState State { get => _playerState.Value; private set => _playerState.Value = value; }
        
        [CanBeNull] public IGrabbable HeldObjMain { get; set; }
        [CanBeNull] public IGrabbable HeldObjSecond { get; set; }


        private void Start()
        {
            if (!IsOwner) return;

            PlayerId = NetworkManager.Singleton.LocalClientId;
            State = PlayerState.Survivor;
        }
    }
}