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
                
                foreach (var door in targetPlate.Doors)
                {
                    if (existingDirection + door.GlobalDirection == Vector3Int.zero)
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
            }

            return new AlignmentResult { Success = false };
        }
    }
}