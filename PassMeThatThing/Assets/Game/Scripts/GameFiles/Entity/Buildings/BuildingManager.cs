using System;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class BuildingManager : NetworkBehaviour
    {
        [Inject] private BuildingsDatabase _buildingsDatabase;
        
        
        
        [Command(requiresAuthority =  false)]
        public void CmdSpawnBuilding(Vector3 pos, int buildingIndex)
        {
            if (_buildingsDatabase.allBuildings.Count > buildingIndex)
            {
                if (true) // проверка на ресурсы
                {
                    var buildingData = _buildingsDatabase.allBuildings[buildingIndex];
                    SpawnBuilding(pos, buildingData);
                    
                }
            }
            
        }
        
        [Server]
        public void SpawnBuilding(Vector3 pos, BuildingData buildingData)
        {
            var instance = Instantiate(buildingData.worldPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(instance);
            Debug.Log($"Spawned building {buildingData.id}");
        }
    }
}