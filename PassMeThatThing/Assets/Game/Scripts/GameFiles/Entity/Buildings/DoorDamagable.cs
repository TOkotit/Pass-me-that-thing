using Entity;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class DoorDamagable : Damagable
    {
        public override DamagableModel DamagableModel { get; }
        public override void OnDeath()
        {
            throw new System.NotImplementedException();
        }

        public override void OnHealthChanged(int currentHealth)
        {
            throw new System.NotImplementedException();
        }
    }
}