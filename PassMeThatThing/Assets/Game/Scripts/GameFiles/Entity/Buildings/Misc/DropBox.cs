using System;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class DropBox : Furniture, Interactable
    {
        public static Dictionary<GameObject, DropBox> Boxes = new Dictionary<GameObject, DropBox>();

        [SerializeField] private ItemSpawner spawner;
        [SerializeField] private NetworkItem networkItem;
        [Inject] private ItemPoolManager _itemPoolManager;

        private List<ItemData> items = new List<ItemData>();
        private Dictionary<Resource, float> _resources = new Dictionary<Resource, float>();

        public List<ItemData> Items => items;
        public Dictionary<Resource, float> Resources => _resources;

        public void Interact() { }
        public void SrbToggle() { }

        public void InteractWithItem(PhysicalItem item)
        {
            items.Add(item.Network.ItemData);
            var inv = item.Owner.MainCharacterModel.PlayerInventory;
            if (inv) inv.ServerRemoveItemFromOwner(item);
            foreach (var resource in item.Resources)
            {
                _resources[resource.Key] = resource.Value;
            }
            _itemPoolManager.ReturnToPool(item.Network);
        }

        public override void OnDeath()
        {
            if (!isServer) return;

            foreach (var item in items)
            {
                spawner.Item = item;
                spawner.Interact();
            }
            items.Clear();
            Boxes.Remove(gameObject);
            StartCoroutine(DelayedUnspawn());
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            Debug.LogWarning("Health: " + currentHealth + "/" + maxHealth);
        }

        private void Awake()
        {
            Boxes[gameObject] = this;
            if (_toughnessModel == null)
                _toughnessModel = new ToughnessModel();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }

        private IEnumerator DelayedUnspawn()
        {
            yield return new WaitForEndOfFrame();

            if (networkItem && _itemPoolManager)
            {
                _itemPoolManager.DeleteAndDestroyObject(networkItem);
            }
            else
            {
                NetworkServer.Destroy(gameObject);
            }

            Boxes.Remove(gameObject);
        }

        private void OnEnable()
        {
            if (DamagableModel != null && DamagableModel.HealthPool != null)
                DamagableModel.SetHealth(DamagableModel.HealthPool.MaxHealth);
        }
    }
}