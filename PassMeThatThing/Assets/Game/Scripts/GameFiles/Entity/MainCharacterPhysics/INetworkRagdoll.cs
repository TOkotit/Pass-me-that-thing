using Entity;
using Game.Entity;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public interface INetworkRagdoll
    {
        public void Setup(Damagable damagable);
        public void EnableRagdoll();
        public void DisableRagdoll();
    }
}