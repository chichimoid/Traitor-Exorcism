using UnityEngine;
using UnityEngine.UI;

namespace PlayerScripts
{
    /// <summary>
    /// For now, only used with text emotes. Everything to be reconfigured for actual emotes.
    /// </summary>
    public class EmoteButton : MonoBehaviour
    {
        [SerializeField] private string emoteName;
        public string EmoteName => emoteName;

        public Button Button { get; private set; }

        private void Awake()
        {
            Button = GetComponent<Button>();
        }
    }
}