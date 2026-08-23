using Entity;

namespace Game.Scripts.GameFiles.Entity
{
    public interface IStatusEffect
    {
        public float TickRate { get; set; }
        public int Stacks { get; set; }
        public void OnReapply(Damageable damageable);
        public void OnApply(Damageable damageable, int stackCount);
        public void OnEndEffect(Damageable damageable);
        public void OnTick(Damageable damageable);
    }
}