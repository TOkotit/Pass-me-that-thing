using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Items.Refill
{
    public interface IRefillable
    {
        public RefillType RefillType { get; }
        public void Refill();
    }
}