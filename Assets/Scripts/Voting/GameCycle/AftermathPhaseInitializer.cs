using PlayerScripts;
using PlayerScripts.UI;
using Unity.Netcode;

namespace Voting.GameCycle
{
    public class AftermathPhaseInitializer : NetworkBehaviour
    {
        public void Init(ulong votedPlayerId)
        {
            ShowResultsRpc(GameManager.GetMonsterId() == votedPlayerId, votedPlayerId);
        }
        
        [Rpc(SendTo.Everyone)]
        private void ShowResultsRpc(bool guessedCorrectly, ulong votedPlayerId)
        {
            PlayerLocker.Instance.LockActivatableUI();
            PlayerLocker.Instance.UnlockCursor();
            var role =  NetworkPlayer.GetLocalInstance().Role;
            PlayerUI.Instance.EndScreenUI.Show(guessedCorrectly ^ role == PlayerRole.Monster, votedPlayerId);
        }
    }
}