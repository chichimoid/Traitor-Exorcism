using Unity.Netcode;

namespace ObjectScripts
{
    public abstract class Weapon : Grabbable
    {
         public abstract float Damage { get; protected set; }
    }
}