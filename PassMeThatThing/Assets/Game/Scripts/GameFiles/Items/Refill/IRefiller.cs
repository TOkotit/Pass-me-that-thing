using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Items.Refill
{
    public interface IRefiller
    {
        public RefillType RefillType { get; }
        public int RefillAmount { get; }
        public bool DropOnEmpty { get; }
    }
}