using System;

namespace Game.Scripts.Enums
{
    [Flags]
    public enum BuildingPlacementType
    {
        None = 0,
        Floor = 1 << 0,
        Walls = 1 << 1,
        Ceiling = 1 << 2
    }
}