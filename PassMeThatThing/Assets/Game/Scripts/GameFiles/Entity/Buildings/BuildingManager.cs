using System;
using Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants;
using Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data;
using DG.Tweening;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class BuildingManager : NetworkBehaviour
    {
        [Inject] private BuildingsDatabase _buildingsDatabase;
        [Inject] private PlantDatabase _plantDatabase;


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

        [Command(requiresAuthority = false)]
        public void CmdDestroyBuilding(Building obj)
        {
            obj.RpcScaleAndDestroyBuilding();
        }

        [Command(requiresAuthority = false)]
        public void CmdDestroyGameObject(GameObject obj)
        {
            RpcScaleAndDestroyGameObject(obj);
        }


        [Server]
        private void DestroyGameObject(GameObject obj)
        {
            NetworkServer.Destroy(obj);
        }

        [ClientRpc]
        public void RpcScaleAndDestroyGameObject(GameObject obj)
        {
            if (isServer)
                transform.DOScale(0f, 0.3f).OnComplete(() => DestroyGameObject(obj));
            else
                transform.DOScale(0f, 0.3f);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetSeed(Farm farm, string seedId)
        {
            SetSeed(farm, seedId);
        }

        [Server]
        public void SetSeed(Farm farm, string seedId)
        {
            farm.SetSeed(_plantDatabase.GetSeed(seedId));
        }
    }
}