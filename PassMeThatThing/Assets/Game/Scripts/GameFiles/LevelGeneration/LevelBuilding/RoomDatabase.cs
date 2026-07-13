using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    [CreateAssetMenu(fileName = "RoomDatabase", menuName = "Level Generation/Room Database")]
    public class RoomDatabase: ScriptableObject
    {
        public List<LevelRoom> hubRooms = new List<LevelRoom>();
        public List<LevelRoom> exitRooms = new List<LevelRoom>();
        public List<LevelRoom> regularRooms = new List<LevelRoom>();
        public List<LevelRoom> defenseRooms = new List<LevelRoom>();
        
        public List<LevelRoom> floodBrokenPumpRooms = new List<LevelRoom>();
        public List<LevelRoom> floodPipeBreakRooms = new List<LevelRoom>();
        
        public List<LevelRoom> blackoutBlowFuseRooms = new List<LevelRoom>();
        public List<LevelRoom> blackoutCutWiresRooms = new List<LevelRoom>();
        
        
        public List<LevelRoom> GetSuitableRooms(RoomType type, int requiredConnections, GameEventsType? targetEventType = null, bool exactMatch = true)
        {
            var targetList = GetTargetList(type, targetEventType);
            var suitableRooms = new List<LevelRoom>();

            if (targetList == null || targetList.Count == 0)
                return suitableRooms;

            for (var i = 0; i < targetList.Count; i++)
            {
                var room = targetList[i];
                if (room == null) continue;
                var totalRoomConnections = room.TotalDoors + room.TotalGates;
                var connectionsMatch = exactMatch 
                    ? totalRoomConnections == requiredConnections 
                    : totalRoomConnections >= requiredConnections;

                if (connectionsMatch)
                {
                    suitableRooms.Add(room);
                }
            }

            return suitableRooms;
        }
        
        private List<LevelRoom> GetTargetList(RoomType type, GameEventsType? targetEventType)
        {
            switch (type)
            {
                case RoomType.Hub: return hubRooms;
                case RoomType.Exit: return exitRooms;
                case RoomType.Regular: return regularRooms;
                case RoomType.Defense: return defenseRooms;
                case RoomType.Event:
                    if (!targetEventType.HasValue) return null;

                    return targetEventType.Value switch
                    {
                        GameEventsType.FloodBrokenPump => floodBrokenPumpRooms,
                        GameEventsType.FloodPipeBreak => floodPipeBreakRooms,
                        GameEventsType.BlackoutBlowFuse => blackoutBlowFuseRooms,
                        GameEventsType.BlackoutCutWires => blackoutCutWiresRooms,
                        _ => null
                    };
                default: return null;
            }
        }
        
    }
}