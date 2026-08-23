using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public enum RoomRotation
    {
        Deg0 = 0,
        Deg90 = 1,
        Deg180 = 2,
        Deg270 = 3
    }
    
    public static class RoomRotationHelper
    {
        public static VirtualPlateData[] GetRotatedPlates(RoomDataEntry room, RoomRotation rotation)
        {
            var originalPlates = room.RoomComponent.Plates;
            var rotatedPlates = new VirtualPlateData[originalPlates.Length];

            for (var i = 0; i < originalPlates.Length; i++)
            {
                var originalPos = originalPlates[i].localPosition;
                var plateRef = originalPlates[i].plate;
                var plateRotation = originalPlates[i].localRotation;

                var newPos = RotateVector(originalPos, rotation);
                var doors = new List<VirtualDoor>();

                if (plateRef.HasDoorNorth)
                    doors.Add(CreateDoor(Vector3Int.forward, plateRotation, rotation));
                    
                if (plateRef.HasDoorEast)
                    doors.Add(CreateDoor(Vector3Int.right, plateRotation, rotation));
                    
                if (plateRef.HasDoorSouth)
                    doors.Add(CreateDoor(Vector3Int.back, plateRotation, rotation));
                    
                if (plateRef.HasDoorWest)
                    doors.Add(CreateDoor(Vector3Int.left, plateRotation, rotation));

                rotatedPlates[i] = new VirtualPlateData
                {
                    LocalPosition = newPos,
                    Doors = doors
                };
            }

            return rotatedPlates;
        }
        
        
        private static VirtualDoor CreateDoor(Vector3Int localDir, RoomRotation plateRotation, RoomRotation roomRotation)

        {
            var directionInRoomFrame = RotateVector(localDir, plateRotation);

            
            return new VirtualDoor
            {
                LocalDirection = directionInRoomFrame,
                GlobalDirection = RotateVector(directionInRoomFrame, roomRotation)
            };

        }
        
        private static Vector3Int RotateVector(Vector3Int vector, RoomRotation rotation)
        {
            return rotation switch
            {
                RoomRotation.Deg0 => vector,
                RoomRotation.Deg90 => new Vector3Int(vector.z, vector.y, -vector.x),
                RoomRotation.Deg180 => new Vector3Int(-vector.x, vector.y, -vector.z),
                RoomRotation.Deg270 => new Vector3Int(-vector.z, vector.y, vector.x),
                _ => vector
            };
        }

    }
}