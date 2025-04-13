using PlayerScripts;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Maze.GameCycle
{
    public class Phase2Initializer : NetworkBehaviour
    {
        public void Init()
        {
            GiveRoles();
        }
        
        private void GiveRoles()
        {
            var monsterIdIndex = Random.Range(0, NetworkManager.Singleton.ConnectedClientsList.Count);
            var monsterId = NetworkManager.Singleton.ConnectedClientsIds[monsterIdIndex];
            
            GiveRolesRpc(monsterId);

            var monsterNetworkPlayer = GameManager.Instance.GetMonsterNetworkPlayer();
            monsterNetworkPlayer.GetComponent<PlayerInfection>().enabled = false;
            var monsterBar = monsterNetworkPlayer.GetComponent<MonsterBar>();
            monsterBar.enabled = true;
            monsterBar.StartFilling();
            
            GlobalDebugger.Instance.Log($"Phase 2 initialized with player {monsterId} being the monster.");
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