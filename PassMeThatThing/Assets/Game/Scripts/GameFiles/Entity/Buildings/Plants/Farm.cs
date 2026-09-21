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
        [SerializeField] private ItemSpawner itemSpawner;

        [Inject] private PlantDatabase plantDatabase;

        private PlantData _currentPlant;
        private bool _isCurrentPlantSet;
        private float _growTimeElapsed;
        private bool _isGrown;

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
            OnGrowTimeElapsedPercentChanged?.Invoke(_growTimeElapsed / _currentPlant.growTime);
            if (_growTimeElapsed >= _currentPlant.growTime)
            {
                _isGrown = true;
                OnIsGrownChanged?.Invoke(_isGrown);
                _growTimeElapsed = 0f;
                GiveFruits();
            }
        }

        [Server]
        private void GiveFruits()
        {
            Debug.Log($"[FARM] {_currentPlant.fruitItem.Id}");
            itemSpawner.ServerSpawnItem(_currentPlant.fruitItem.Id, itemSpawner.transform.position);
        }

        //public void Interact()
        //{

        //}

        //public void SrbToggle()
        //{

        //}

        //public void InteractWithItem(PhysicalItem item)
        //{
            
        //}

        //public override void OnStartClient()
        //{
        //    base.OnStartClient();

        //    InteractableRegistry.Instance.Register(gameObject, this);
        //}
    }
}