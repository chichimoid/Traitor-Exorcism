using UnityEngine;

namespace PlayerScripts
{
    public class PlayerRayCaster : MonoBehaviour
    {
        [SerializeField] private LayerMask raycastObstacle;
        
        private Camera _playerCamera;

        private void Start()
        {
            _playerCamera = Camera.main;
        }
        public bool ViewPointRayCast(out RaycastHit hit, float distance)
        {
            var ray = _playerCamera.ViewportPointToRay(Vector3.one / 2f);
            return Physics.Raycast(ray, out hit, distance);
        }
        
        public bool PlayerObstacleRayCast(out RaycastHit hit, Transform targetPlayer)
        {
            var direction = targetPlayer.position - transform.position;
            float distance = direction.magnitude;
            var ray = new Ray(transform.position, direction);
            return Physics.Raycast(ray, out hit, distance, raycastObstacle);
        }
    }
}
