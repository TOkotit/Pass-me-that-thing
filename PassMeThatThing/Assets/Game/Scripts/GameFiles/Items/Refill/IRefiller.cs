using System.Collections;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;

namespace Game.Scripts.GameFiles.Items.Refill
{
    public interface IRefiller
    {
        public RefillType RefillType { get; }
        public int RefillAmount { get; }
        public bool DropOnEmpty { get; }
        IEnumerator Refill(IRefillable target, PlayerInventory inventory);
    }
}