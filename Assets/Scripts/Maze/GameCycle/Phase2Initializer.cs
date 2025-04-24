using NetworkHelperScripts;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace Maze.GameCycle
{
    public class Phase2Initializer : NetworkBehaviour
    {
        public void Init(out ulong monsterId)
        {
            monsterId = GiveRoles();
        }
        
        private ulong GiveRoles()
        {
            var monsterIdIndex = Random.Range(0, GameManager.Instance.AlivePlayersIds.Count);
            var monsterId = GameManager.Instance.AlivePlayersIds[monsterIdIndex];
            
            GiveRolesRpc(monsterId);

            InitMonsterRpc(monsterId);
            InitMonsterTargetRpc(RpcTarget.Single(monsterId, RpcTargetUse.Temp));
            
            GlobalDebugger.Instance.Log($"Phase 2 initialized with player {monsterId} being the monster.");

            return monsterId;
        }

        [Rpc(SendTo.Everyone)]
        private void InitMonsterRpc(ulong monsterId)
        {
            var monsterNetworkPlayer = NetworkPlayer.GetInstance(monsterId);
            monsterNetworkPlayer.GetComponent<PlayerInfection>().enabled = false;
            var monsterBar = monsterNetworkPlayer.GetComponent<MonsterBar>();
            monsterBar.enabled = true;
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void InitMonsterTargetRpc(RpcParams rpcParams)
        {
            var networkPlayer = NetworkPlayer.GetLocalInstance();
            networkPlayer.GetComponent<MonsterBar>().StartFilling();
            //networkPlayer.GetComponent<PlayerHealth>().StartMonsterHealthDrain();
        }

        [Rpc(SendTo.Everyone)]
        private void GiveRolesRpc(ulong monsterId)
        {
            var networkPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
            if (networkPlayer.Id == monsterId)
            {
                networkPlayer.Role = PlayerRole.Monster;
            }
            else
            {
                networkPlayer.Role = PlayerRole.Survivor;
            }
        }
    }
}