using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Items.Refill
{
    public interface IRefillable
    {
        public RefillType RefillType { get; }
        public int MaxAmmo { get; }
        public int CurrentAmmo { get; set; }
        public float ReloadTime { get; }
    }
}