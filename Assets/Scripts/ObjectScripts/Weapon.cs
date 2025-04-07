using Unity.Netcode;

namespace ObjectScripts
{
    public abstract class Weapon : Usable
    {
         public abstract float Damage { get; protected set; }
    }
}