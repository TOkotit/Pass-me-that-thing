using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    [System.Serializable]
    public struct RoomDataEntry
    {
        public GameObject PrefabGameObject;
        public LevelRoom RoomComponent;
    }
    
    [CreateAssetMenu(fileName = "RoomDatabaseNew", menuName = "Level Generation/Room Database New")]
    public class RoomDatabase : ScriptableObject
    {
        public List<RoomDataEntry> commandCenterRooms = new();
        public List<RoomDataEntry> generatorRooms = new();
        public List<RoomDataEntry> warehouseRooms = new();
        public List<RoomDataEntry> livingBlockRooms = new();
        public List<RoomDataEntry> medicalBlockRooms = new();
        public List<RoomDataEntry> recoveryHangarRooms = new();
        public List<RoomDataEntry> technicalTunnelsRooms = new();
        public List<RoomDataEntry> laboratoryRooms = new();
        public List<RoomDataEntry> workshopRooms = new();
        public List<RoomDataEntry> serverRooms = new();
        public List<RoomDataEntry> waterPurificationRooms = new();
        public List<RoomDataEntry> armoryRooms = new();
        
        public List<RoomDataEntry> GetSuitableRooms(RoomType type, int requiredConnections, bool exactMatch = true)
        {
            var targetList = GetTargetList(type);
            var suitableRooms = new List<RoomDataEntry>();

            if (targetList == null || targetList.Count == 0)
                return suitableRooms;

            foreach (var entry in targetList)
            {
                if (entry.PrefabGameObject == null || entry.RoomComponent == null) continue;
                
                var connectionsMatch = exactMatch 
                    ? entry.RoomComponent.TotalDoors == requiredConnections 
                    : entry.RoomComponent.TotalDoors >= requiredConnections;

                if (connectionsMatch)
                {
                    suitableRooms.Add(entry);
                }
            }

            return suitableRooms;
        }
        
        private List<RoomDataEntry> GetTargetList(RoomType type)
        {
            return type switch
            {
                RoomType.CommandCenter => commandCenterRooms,
                RoomType.Generator => generatorRooms,
                RoomType.Warehouse => warehouseRooms,
                RoomType.LivingBlock => livingBlockRooms,
                RoomType.MedicalBlock => medicalBlockRooms,
                RoomType.RecoveryHangar => recoveryHangarRooms,
                RoomType.TechnicalTunnels => technicalTunnelsRooms,
                RoomType.Laboratory => laboratoryRooms,
                RoomType.Workshop => workshopRooms,
                RoomType.Server => serverRooms,
                RoomType.WaterPurification => waterPurificationRooms,
                RoomType.Armory => armoryRooms,
                _ => null
            };
        }
        
        
        private void OnValidate()
        {
            FillComponents(commandCenterRooms);
            FillComponents(generatorRooms);
            FillComponents(warehouseRooms);
            FillComponents(livingBlockRooms);
            FillComponents(medicalBlockRooms);
            FillComponents(recoveryHangarRooms);
            FillComponents(technicalTunnelsRooms);
            FillComponents(laboratoryRooms);
            FillComponents(workshopRooms);
            FillComponents(serverRooms);
            FillComponents(waterPurificationRooms);
            FillComponents(armoryRooms);
        }
        
        private void FillComponents(List<RoomDataEntry> entries)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.PrefabGameObject != null && entry.RoomComponent == null)
                {
                    entry.RoomComponent = entry.PrefabGameObject.GetComponent<LevelRoom>();
                    entries[i] = entry;
                }
            }
        }
        

    }
}