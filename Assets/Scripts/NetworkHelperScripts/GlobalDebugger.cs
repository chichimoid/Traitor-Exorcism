using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NetworkHelperScripts
{
    public class GlobalDebugger : NetworkBehaviour
    {
        public static GlobalDebugger Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    
        /// <summary>
        /// Has a 256 char limit.
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            LogRpc(message);
        }

        [Rpc(SendTo.Everyone)]
        private void LogRpc(FixedString512Bytes message)
        {
            Debug.Log(message);
        }
    }
}