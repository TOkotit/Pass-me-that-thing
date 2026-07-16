using System;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class DropBox : Damagable, Interactable
    {
        static public Dictionary<GameObject, DropBox> Boxes = new Dictionary<GameObject, DropBox>();
        [SerializeField] private ItemSpawner spawner;
        [Inject] private DamagableModel _model;  
        public override DamagableModel DamagableModel => _model;
        private List<ItemData> items = new List<ItemData>();
        private Dictionary<Resource, int> _resources = new Dictionary<Resource, int>();

        public List<ItemData> Items => items;
        public Dictionary<Resource, int> Resources => _resources;
        public void Interact()
        {
            
        }

        public void SrbToggle()
        {
            
        }

        public void InteractWithItem(PhysicalItem item)
        {
            items.Add(item.Network.ItemData);
            var inv = item.Owner.MainCharacterModel.PlayerInventory;
            if (inv) inv.ServerRemoveItemFromOwner(item);
            foreach (var resource in item.Resources)
            {
                _resources[resource.Key] = resource.Value;
            }
            NetworkServer.UnSpawn(item.gameObject);
        }
        public override void OnDeath()
        {
            if (!isServer) return;
            foreach (var item in items)
            {
                spawner.Item = item;
                Debug.Log("Попытка достать предмет из коробки " +spawner.Item);
                spawner.Interact();
            }
            //NetworkServer.UnSpawn(gameObject);
            StartCoroutine(DelayedUnspawn());
            Boxes.Remove(gameObject);
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            Debug.LogWarning("Health: " + currentHealth + "/" + maxHealth);
        }

        private void Awake()
        { 
            Boxes[gameObject] = this;
            
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
        
        private IEnumerator DelayedUnspawn()
        {
            yield return new WaitForEndOfFrame();   
            NetworkServer.UnSpawn(gameObject);
            Boxes.Remove(gameObject);
        }
    }
}