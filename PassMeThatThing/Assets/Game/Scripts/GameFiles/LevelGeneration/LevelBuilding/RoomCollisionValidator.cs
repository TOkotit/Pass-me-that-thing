using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public static class RoomCollisionValidator
    {
        public static bool IsPlacementValid(LevelGrid grid, RoomDataEntry entry, RoomRotation rotation, Vector3Int origin)
        {
            if (!grid || !entry.PrefabGameObject || !entry.RoomComponent) return false;

            var rotatedPlates = RoomRotationHelper.GetRotatedPlates(entry, rotation);

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