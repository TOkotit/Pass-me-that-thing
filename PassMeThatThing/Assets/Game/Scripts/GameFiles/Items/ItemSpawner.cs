using Ami.BroAudio;
using DG.Tweening;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemSpawner : NetworkBehaviour, Interactable
    {
        [SerializeField] private Transform pointToSpawn;
        [SerializeField] private ItemData item;
        [SerializeField] private SoundSource craftSound;
        [SerializeField] private ItemPoolManager reserveItemPoolManager; //удалить потом
        public ItemData Item {get => item; set => item = value;}
        public Transform PointToSpawn {get => pointToSpawn; set => pointToSpawn = value;}
        
        
        [Inject] private ItemPoolManager _itemPoolManager; 
        [Inject] private PhysicalItemRegistry _physicalItemRegistry;

        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }

        [Server]
        public void ServerSpawnCurrentItem()
        {
            var itemToDrop = _itemPoolManager.CreateNewObject(item.Id);
            itemToDrop.transform.position = pointToSpawn.position;

            var physItem = itemToDrop.GetComponent<PhysicalItem>();
            _physicalItemRegistry.Register(physItem);
            RpcCraftPlaySound();
            //RpcInteractWithObject();
        }

        [Server]
        public void ServerSpawnItem(string itemId, Vector3 pos)
        {
            var itemToDrop = _itemPoolManager.CreateNewObject(itemId);
            itemToDrop.transform.position = pos;

            var physItem = itemToDrop.GetComponent<PhysicalItem>();
            _physicalItemRegistry.Register(physItem);
        }

        [Server]
        public void ServerSpawnItemsFromChanceDict(Dictionary<ItemData, float> drops, Vector3 pos)
        {
            foreach (var d in drops)
            {
                var r = Random.Range(0, 100) / 100f;
                if (r <= d.Value)
                {
                    ServerSpawnItem(d.Key.Id, pos);
                    Debug.Log($"спавн дропа - {d.Key.Id} шанс {r} <= {d.Value}");
                }
            }
        }

        [ClientRpc]
        public void RpcInteractWithObject()
        {
            gameObject.transform.DOScale(0f, 0.5f).SetEase(Ease.InBounce)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
        }
        
        [ClientRpc]
        public void RpcCraftPlaySound()
        {
            if (craftSound)
            {
                craftSound.Play();
            }
        }

        [Obsolete]
        public void SpawnItem() //код для тестовой сцены, не работает. потом удалить
        {
            if (!_itemPoolManager || _physicalItemRegistry == null)
            {
                _itemPoolManager = reserveItemPoolManager;
                _physicalItemRegistry = PhysicalItemRegistry.Instance;
            }

            var itemToDrop = _itemPoolManager.CreateNewObject(item.Id);
            if (!itemToDrop) return;
            itemToDrop.transform.position = pointToSpawn.position;
            itemToDrop.SetActive(true);
            var physItem = itemToDrop.GetComponent<PhysicalItem>();
            if (physItem)
                _physicalItemRegistry.Register(physItem);
        }

        public void Interact()
        {
            CmdInteractWithObject();
        }

        [Command(requiresAuthority = false)]
        private void CmdInteractWithObject()
        {
            ServerSpawnCurrentItem();
        }

        public void SrbToggle()
        {
            
        }

        public void InteractWithItem(PhysicalItem item)
        {
            
        }

    }
}