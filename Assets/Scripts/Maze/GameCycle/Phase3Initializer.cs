using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace Maze.GameCycle
{
    public class Phase3Initializer : NetworkBehaviour
    {
        public void Init()
        {
            LeverManager.Instance.ShowLevers();
            
            SetMonsterMeshTargetRpc(RpcTarget.Single(GameManager.GetMonsterId(), RpcTargetUse.Temp));
            
            GlobalDebugger.Instance.Log($"Phase 3 initialized.");
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SetMonsterMeshTargetRpc(RpcParams rpcParams)
        {
            var networkPlayer = NetworkPlayer.GetLocalInstance();
            var playerMesh = networkPlayer.GetComponent<PlayerMesh>();
            playerMesh.SetMonsterMaterial();
        }
    }
}