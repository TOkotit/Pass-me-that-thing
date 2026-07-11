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

                var newPos = RotateGridPosition(originalPos, rotation);

                var n = plateRef.ConnectionNorth;
                var e = plateRef.ConnectionEast;
                var s = plateRef.ConnectionSouth;
                var w = plateRef.ConnectionWest;

                RoomsConnectionTypes newN = n, newE = e, newS = s, newW = w;

                switch (rotation)
                {
                    case RoomRotation.Deg90:
                        newN = w; 
                        newE = n; 
                        newS = e; 
                        newW = s;
                        break;
                    
                    case RoomRotation.Deg180:
                        newN = s; 
                        newE = w; 
                        newS = n; 
                        newW = e;
                        break;
                    
                    case RoomRotation.Deg270:
                        newN = e; 
                        newE = s; 
                        newS = w; 
                        newW = n;
                        break;
                }

                rotatedPlates[i] = new VirtualPlateData
                {
                    LocalPosition = newPos,
                    ConnectionNorth = newN,
                    ConnectionEast = newE,
                    ConnectionSouth = newS,
                    ConnectionWest = newW
                };
            }

            return rotatedPlates;
        }
        
        private static Vector3Int RotateGridPosition(Vector3Int position, RoomRotation rotation)
        {
            return rotation switch
            {
                RoomRotation.Deg0 => position,
                
                RoomRotation.Deg90 => new Vector3Int(position.z, position.y, -position.x),
                
                RoomRotation.Deg180 => new Vector3Int(-position.x, position.y, -position.z),
                
                RoomRotation.Deg270 => new Vector3Int(-position.z, position.y, position.x),
                
                _ => position
            };
        }
    }
}