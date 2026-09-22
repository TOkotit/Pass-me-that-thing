using Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants.Data;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Plants
{
    public class Farm : NetworkBehaviour
    {
        [SerializeField] private FarmView farmView;

        [SerializeField] private Transform saplingPosition;

        [SerializeField] private ItemSpawner itemSpawner;


        //[Inject] private PlantDatabase plantDatabase;

        private PlantData _currentPlant;
        private bool _isCurrentPlantSet;
        private float _growTimeElapsed;
        private bool _isGrown;

        public FarmView FarmView => farmView;

        public event Action<float> OnGrowTimeElapsedPercentChanged;
        public event Action<bool> OnIsGrownChanged;

        public void FixedUpdate()
        {
            if (!isServer) return;
            if (!_isCurrentPlantSet) return;

            if (!_isGrown)
            {
                GrowTick();
            }
        }

        public void Start()
        {
            farmView.InitUI(this);
        }

        [Server]
        public void SetSeed(PlantSeedData seedData)
        {
            ResetFarm();
            SetPlant(seedData.plants.First());
        }

        [Server]
        public void ResetFarm()
        {
            _isCurrentPlantSet = false;
            _growTimeElapsed = 0f;
            _isGrown = false;
            RpcInvokeGrowPercent(0f);
        }

        [Server]
        public void SetPlant(PlantData plantData)
        {
            _currentPlant = plantData;
            _isCurrentPlantSet = true;
        }

        [Server]
        private void GrowTick()
        {
            _growTimeElapsed += Time.fixedDeltaTime;
            RpcInvokeGrowPercent(_growTimeElapsed / _currentPlant.growTime);
            if (_growTimeElapsed >= _currentPlant.growTime)
            {
                _isGrown = true;
                OnIsGrownChanged?.Invoke(_isGrown);
                _growTimeElapsed = 0f;
                GiveFruits();
            }
        }

        [ClientRpc]
        public void RpcInvokeGrowPercent(float v)
        {
            OnGrowTimeElapsedPercentChanged?.Invoke(v);
        }

        [Server]
        private void GiveFruits()
        {
            Debug.Log($"[FARM] {_currentPlant.fruitItem.Id}");
            for (var i=0; i < _currentPlant.fruitsAmount; i++)
            {
                itemSpawner.ServerSpawnItem(_currentPlant.fruitItem.Id, saplingPosition.position);
            }
            ResetFarm();
        }
    }
}