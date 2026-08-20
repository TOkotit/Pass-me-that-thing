using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public static class RoomCollisionValidator
    {
        public static bool IsPlacementValid(LevelGrid grid, LevelRoomNew room, RoomRotation rotation, Vector3Int origin)
        {
            if (grid == null || room == null) return false;

            var rotatedPlates = RoomRotationHelper.GetRotatedPlates(room, rotation);

            for (var i = 0; i < rotatedPlates.Length; i++)
            {
                var globalCellPos = origin + rotatedPlates[i].LocalPosition;

                if (grid.IsCellOccupied(globalCellPos))
                {
                    return false; 
                }
            }

            return true;
        }
    }
}