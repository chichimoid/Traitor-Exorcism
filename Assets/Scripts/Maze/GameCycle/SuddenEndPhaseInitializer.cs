using PlayerScripts;
using PlayerScripts.UI;
using Unity.Netcode;

namespace Maze.GameCycle
{
    public class SuddenEndPhaseInitializer : NetworkBehaviour
    {
        public void Init(bool survivorsWon, ulong monsterId)
        {
            ShowResultsRpc(survivorsWon, monsterId);
        }
        
        [Rpc(SendTo.Everyone)]
        private void ShowResultsRpc(bool survivorsWon, ulong monsterId)
        {
            PlayerLocker.Instance.LockMovement();
            PlayerLocker.Instance.LockActivatableUI();
            PlayerLocker.Instance.LockActions();
            PlayerLocker.Instance.UnlockCursor();
            var networkPlayer = NetworkPlayer.GetLocalInstance();
            networkPlayer.GetComponent<PlayerInfection>().enabled = false;
            networkPlayer.GetComponent<PlayerHealth>().enabled = false;
            var role =  NetworkPlayer.GetLocalInstance().Role;
            PlayerUI.Instance.SuddenEndScreenUI.Show(survivorsWon ^ role == PlayerRole.Monster, monsterId);
        }
    }
}