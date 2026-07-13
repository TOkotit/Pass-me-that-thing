using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public struct VirtualDoor
    {
        public Vector3Int LocalDirection;  
        public Vector3Int GlobalDirection; 
        public RoomsConnectionTypes Type;
    }

    public struct VirtualPlateData
    {
        public Vector3Int LocalPosition;
        public List<VirtualDoor> Doors;
    }
}