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
        public static VirtualPlateData[] GetRotatedPlates(LevelRoom room, RoomRotation rotation)
        {
            var originalPlates = room.Plates;
            var rotatedPlates = new VirtualPlateData[originalPlates.Length];

            for (var i = 0; i < originalPlates.Length; i++)
            {
                var originalPos = originalPlates[i].localPosition;
                var plateRef = originalPlates[i].plate;

                var newPos = RotateVector(originalPos, rotation);
                var doors = new List<VirtualDoor>();

                if (plateRef.ConnectionNorth != RoomsConnectionTypes.None)
                    doors.Add(CreateDoor(Vector3Int.forward, plateRef.ConnectionNorth, rotation));
                    
                if (plateRef.ConnectionEast != RoomsConnectionTypes.None)
                    doors.Add(CreateDoor(Vector3Int.right, plateRef.ConnectionEast, rotation));
                    
                if (plateRef.ConnectionSouth != RoomsConnectionTypes.None)
                    doors.Add(CreateDoor(Vector3Int.back, plateRef.ConnectionSouth, rotation));
                    
                if (plateRef.ConnectionWest != RoomsConnectionTypes.None)
                    doors.Add(CreateDoor(Vector3Int.left, plateRef.ConnectionWest, rotation));

                rotatedPlates[i] = new VirtualPlateData
                {
                    LocalPosition = newPos,
                    Doors = doors
                };
            }

            return rotatedPlates;
        }
        
        public static VirtualPlateData[] GetRotatedPlates(LevelRoomNew room, RoomRotation rotation)
        {
            var originalPlates = room.Plates;
            var rotatedPlates = new VirtualPlateData[originalPlates.Length];

            for (var i = 0; i < originalPlates.Length; i++)
            {
                var originalPos = originalPlates[i].localPosition;
                var plateRef = originalPlates[i].plate;

                var newPos = RotateVector(originalPos, rotation);
                var doors = new List<VirtualDoor>();

                if (plateRef.HasDoorNorth)
                    doors.Add(CreateDoor(Vector3Int.forward, rotation));
                    
                if (plateRef.HasDoorEast)
                    doors.Add(CreateDoor(Vector3Int.right, rotation));
                    
                if (plateRef.HasDoorSouth)
                    doors.Add(CreateDoor(Vector3Int.back, rotation));
                    
                if (plateRef.HasDoorWest)
                    doors.Add(CreateDoor(Vector3Int.left, rotation));

                rotatedPlates[i] = new VirtualPlateData
                {
                    LocalPosition = newPos,
                    Doors = doors
                };
            }

            return rotatedPlates;
        }
        
        private static VirtualDoor CreateDoor(Vector3Int localDir, RoomsConnectionTypes type, RoomRotation rotation)
        {
            return new VirtualDoor
            {
                LocalDirection = localDir,
                GlobalDirection = RotateVector(localDir, rotation),
                Type = type
            };
        }
        private static VirtualDoor CreateDoor(Vector3Int localDir, RoomRotation rotation)
        {
            return new VirtualDoor
            {
                LocalDirection = localDir,
                GlobalDirection = RotateVector(localDir, rotation)
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