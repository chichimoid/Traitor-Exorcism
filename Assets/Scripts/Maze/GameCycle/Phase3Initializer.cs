using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;

namespace Maze.GameCycle
{
    public class Phase3Initializer : NetworkBehaviour
    {
        public void Init()
        {
            LeverManager.Instance.ShowLevers();
            
            TurnToMonsterTargetRpc(RpcTarget.Single(GameManager.GetMonsterId(), RpcTargetUse.Temp));
            
            GlobalDebugger.Instance.Log($"Phase 3 initialized.");
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void TurnToMonsterTargetRpc(RpcParams rpcParams)
        {
            var networkPlayer = NetworkPlayer.GetLocalInstance();
            var playerMesh = networkPlayer.GetComponent<PlayerMesh>();
            playerMesh.SetMonsterMaterial();
            var playerAttack = networkPlayer.GetComponent<PlayerAttack>();
            playerAttack.ToMonsterMultiplier();
            var playerMovement = networkPlayer.GetComponent<PlayerMovement>();
            playerMovement.ToMonsterMultiplier();
        }
    }
}