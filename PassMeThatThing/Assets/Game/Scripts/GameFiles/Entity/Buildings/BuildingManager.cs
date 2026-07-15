using System;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class BuildingManager : NetworkBehaviour
    {
        [Inject] private BuildingsDatabase _buildingsDatabase;
        
        

        // [Command(requiresAuthority =  false)]
        // public void CmdSpawnBuilding(Vector3 pos, int buildingIndex)
        // {
        //     if (_buildingsDatabase.buildings.Count > buildingIndex)
        //     {
        //         var buildingData = _buildingsDatabase.buildings[buildingIndex];
        //         if (true) // проверка на ресурсы
        //         {
        //
        //             SpawnBuilding(pos, buildingData);
        //
        //         }
        //     }
        // }
        
        [Command(requiresAuthority =  false)]
        public void CmdSpawnBuilding(Vector3 pos, Quaternion rotation, string buildingId)
        {
            var buildingData = _buildingsDatabase.GetBuildingFromAll(buildingId);
            
            
            SpawnBuilding(pos,rotation, buildingData);
            
        }
        
        [Server]
        public void SpawnBuilding(Vector3 pos, Quaternion rotation, BuildingData buildingData)
        {
            var instance = Instantiate(buildingData.worldPrefab, pos, rotation);
            NetworkServer.Spawn(instance);
            Debug.Log($"Spawned building {buildingData.id}");
        }
    }
}