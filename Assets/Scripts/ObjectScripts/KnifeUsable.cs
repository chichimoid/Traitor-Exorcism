using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;
using PlayerScripts;

namespace ObjectScripts
{
    public class KnifeUsable : Weapon
    {
        public override float Damage { get; protected set; } = 20f;
        private float _range = 1.5f;            // ������ �����
        private float _stabDistance = 0.3f;     // ��������� ����������� ����
        private float _stabSpeed = 0.1f;        // �������� ��������

        private Transform _currentHoldPoint;
        private Vector3 _initialHoldLocalPos;
        private bool _isAnimating = false;

        protected override void UseFunctional()
        {
            if (!_isAnimating)
            {
                var playerInteract = Player.GetComponent<PlayerInteract>();

                _currentHoldPoint = playerInteract.HoldPointTransformSecond == this
                    ? playerInteract.HoldPointTransformSecond
                    : playerInteract.HoldPointTransformMain;

                _initialHoldLocalPos = _currentHoldPoint.localPosition;
                StartCoroutine(StabHoldPointAnimation());
            }

            if (IsOwner)
                TryHitServerRpc();
        }

        private IEnumerator StabHoldPointAnimation()
        {
            _isAnimating = true;

            Vector3 targetPos = _initialHoldLocalPos + Vector3.forward * _stabDistance;
            float elapsed = 0f;

            // �����
            while (elapsed < _stabSpeed)
            {
                _currentHoldPoint.localPosition = Vector3.Lerp(_initialHoldLocalPos, targetPos, elapsed / _stabSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _currentHoldPoint.localPosition = targetPos;

            elapsed = 0f;

            // �����
            while (elapsed < _stabSpeed)
            {
                _currentHoldPoint.localPosition = Vector3.Lerp(targetPos, _initialHoldLocalPos, elapsed / _stabSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _currentHoldPoint.localPosition = _initialHoldLocalPos;

            _isAnimating = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryHitServerRpc()
        {
            var hits = Physics.OverlapSphere(transform.position + transform.forward * 0.8f, _range);

            var nearestPlayer = hits
                .Select(h => h.GetComponent<PlayerHealth>())
                .Where(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
                .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
                .FirstOrDefault();

            if (nearestPlayer != null)
            {
                nearestPlayer.DamageHealth(Damage);
            }
        }
    }
}