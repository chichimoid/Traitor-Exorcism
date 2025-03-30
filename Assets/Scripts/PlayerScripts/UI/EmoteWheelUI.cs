using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerScripts
{
    public class EmoteWheelUI : MonoBehaviour
    {
        [FormerlySerializedAs("buttons")] [SerializeField] private List<EmoteButton> emoteButtons;
        
        public List<EmoteButton> EmoteButtons => emoteButtons;
    }
}
