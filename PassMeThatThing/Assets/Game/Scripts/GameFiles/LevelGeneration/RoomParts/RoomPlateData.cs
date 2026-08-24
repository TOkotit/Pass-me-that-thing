using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    [System.Serializable]
    public struct RoomPlateData
    {
        public Vector3Int localPosition;
        public RoomPlate plate;
        public RoomRotation localRotation;
    }
}