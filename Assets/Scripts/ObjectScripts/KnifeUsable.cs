using UnityEngine;

namespace ObjectScripts
{
    public class KnifeUsable : Weapon
    {
        private int _count = 3;
        public override float Damage { get; protected set; } = 20f;
        protected override void UseFunctional()
        {
            Debug.Log($"Deal damage to near player: {Damage}");
            // ��������� �������� ����
        }
    }
}