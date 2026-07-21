using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    [CreateAssetMenu(fileName = "RoomDatabaseNew", menuName = "Level Generation/Room Database New")]

    public class RoomDatabaseNew : ScriptableObject
    {
        public List<LevelRoomNew> commandCenterRooms = new();
        public List<LevelRoomNew> generatorRooms = new();
        public List<LevelRoomNew> warehouseRooms = new();
        public List<LevelRoomNew> livingBlockRooms = new();
        public List<LevelRoomNew> medicalBlockRooms = new();
        public List<LevelRoomNew> recoveryHangarRooms = new();
        public List<LevelRoomNew> technicalTunnelsRooms = new();
        
        public List<LevelRoomNew> laboratoryRooms = new();
        public List<LevelRoomNew> workshopRooms = new();
        public List<LevelRoomNew> serverRooms = new();
        public List<LevelRoomNew> waterPurificationRooms = new();
        public List<LevelRoomNew> armoryRooms = new();
        
        public List<LevelRoomNew> GetSuitableRooms(RoomTypeNew type, int requiredConnections, bool exactMatch = true)
        {
            var targetList = GetTargetList(type);
            var suitableRooms = new List<LevelRoomNew>();

            if (targetList == null || targetList.Count == 0)
                return suitableRooms;

            foreach (var room in targetList)
            {
                if (room == null) continue;
                
                var connectionsMatch = exactMatch 
                    ? room.TotalDoors == requiredConnections 
                    : room.TotalDoors >= requiredConnections;

                if (connectionsMatch)
                {
                    suitableRooms.Add(room);
                }
            }

            return suitableRooms;
        }
        private List<LevelRoomNew> GetTargetList(RoomTypeNew type)
        {
            return type switch
            {
                RoomTypeNew.CommandCenter => commandCenterRooms,
                RoomTypeNew.Generator => generatorRooms,
                RoomTypeNew.Warehouse => warehouseRooms,
                RoomTypeNew.LivingBlock => livingBlockRooms,
                RoomTypeNew.MedicalBlock => medicalBlockRooms,
                RoomTypeNew.RecoveryHangar => recoveryHangarRooms,
                RoomTypeNew.TechnicalTunnels => technicalTunnelsRooms,
                RoomTypeNew.Laboratory => laboratoryRooms,
                RoomTypeNew.Workshop => workshopRooms,
                RoomTypeNew.Server => serverRooms,
                RoomTypeNew.WaterPurification => waterPurificationRooms,
                RoomTypeNew.Armory => armoryRooms,
                _ => null
            };
        }
    }
}