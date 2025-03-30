using Unity.Cinemachine;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCam;
        // Regular camera is referenced via Camera.main.
        public static PlayerCamera Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }
        public void Follow(Transform target)
        {
            cinemachineCam.Follow = target;
        }
    }
}
