using System.Collections.Generic;
using UnityEngine;

namespace PlayerScripts.UI
{
    public class EmoteContainer : MonoBehaviour
    {
        [SerializeField] private List<EmoteButton> emoteButtons;
        
        public List<EmoteButton> EmoteButtons => emoteButtons;
    }
}
