using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public struct VirtualPlateData
    {
        public Vector3Int LocalPosition;
        
        public RoomsConnectionTypes ConnectionNorth;
        public RoomsConnectionTypes ConnectionEast;
        public RoomsConnectionTypes ConnectionSouth;
        public RoomsConnectionTypes ConnectionWest;
    }
}