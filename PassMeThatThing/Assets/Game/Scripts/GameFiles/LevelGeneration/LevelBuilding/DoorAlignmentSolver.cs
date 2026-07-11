using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public static class DoorAlignmentSolver
    {
        public static AlignmentResult CalculateAlignment(Vector3Int existingGlobalPos, Vector3Int existingDirection, LevelRoom newRoom, int newRoomPlateIndex)
        {
            var targetConnectionPoint = existingGlobalPos + existingDirection;

            for (var i = 0; i < 4; i++)
            {
                var rotation = (RoomRotation)i;
                var rotatedPlates = RoomRotationHelper.GetRotatedPlates(newRoom, rotation);
                
                var targetPlate = rotatedPlates[newRoomPlateIndex];
                
                var plateDir = GetPlateDirection(targetPlate);

                if (existingDirection + plateDir == Vector3Int.zero)
                {
                    var calculatedOrigin = targetConnectionPoint - targetPlate.LocalPosition;

                    return new AlignmentResult
                    {
                        Success = true,
                        Rotation = rotation,
                        Origin = calculatedOrigin
                    };
                }
            }

            return new AlignmentResult { Success = false };
        }
        
        private static Vector3Int GetPlateDirection(VirtualPlateData plate)
        {
            if (plate.ConnectionNorth != RoomsConnectionTypes.None) return Vector3Int.forward;
            if (plate.ConnectionSouth != RoomsConnectionTypes.None) return Vector3Int.back;
            if (plate.ConnectionEast != RoomsConnectionTypes.None) return Vector3Int.right;
            if (plate.ConnectionWest != RoomsConnectionTypes.None) return Vector3Int.left;
            
            return Vector3Int.zero;
        }
    }
}