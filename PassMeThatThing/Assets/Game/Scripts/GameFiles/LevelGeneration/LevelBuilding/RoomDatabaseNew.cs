using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    [CreateAssetMenu(fileName = "RoomDatabaseNew", menuName = "Level Generation/Room Database New")]

    public class RoomDatabaseNew : ScriptableObject
    {
        public List<LevelRoom> commandCenterRooms = new();
        public List<LevelRoom> generatorRooms = new();
        public List<LevelRoom> warehouseRooms = new();
        public List<LevelRoom> livingBlockRooms = new();
        public List<LevelRoom> medicalBlockRooms = new();
        public List<LevelRoom> recoveryHangarRooms = new();
        public List<LevelRoom> technicalTunnelsRooms = new();
        
        public List<LevelRoom> laboratoryRooms = new();
        public List<LevelRoom> workshopRooms = new();
        public List<LevelRoom> serverRooms = new();
        public List<LevelRoom> waterPurificationRooms = new();
        public List<LevelRoom> armoryRooms = new();
        
        public List<LevelRoom> GetSuitableRooms(RoomTypeNew type, int requiredConnections, bool exactMatch = true)
        {
            var targetList = GetTargetList(type);
            var suitableRooms = new List<LevelRoom>();

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
        private List<LevelRoom> GetTargetList(RoomTypeNew type)
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