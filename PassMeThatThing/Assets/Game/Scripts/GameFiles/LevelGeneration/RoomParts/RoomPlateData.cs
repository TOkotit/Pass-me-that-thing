using System;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    [Serializable]
    public struct RoomPlateData
    {
        public Vector3Int localPosition;
        public RoomPlate plate;
    }
}