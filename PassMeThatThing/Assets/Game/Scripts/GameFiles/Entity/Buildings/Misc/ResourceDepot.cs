using System;
using System.Linq;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class ResourceDepot : NetworkBehaviour
    {
        [SerializeField] protected ResourceStorage storage;
        [Inject] private PhysicalItemRegistry registry;
        [Inject] private ItemPoolManager _itemPoolManager;

        private float lastTransfer;  
        private float transferInterval = 0.5f;

        public void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            Debug.Log("В приемник что-то попало");

            if (other.CompareTag("Item"))
            {
                if (!registry.TryGetItem(other.gameObject, out var item)) return;
                if (item.Resources.Count == 0) return;

                if (item.Owner)
                {
                    var inv = item.Owner.MainCharacterModel.PlayerInventory;
                    if (inv) inv.ServerRemoveItemFromOwner(item);
                }

                foreach (var resourcePair in item.Resources)
                {
                    storage.AddResource(resourcePair.Key, resourcePair.Value);
                }

                item.Owner?.MainCharacterModel.PlayerInteraction
                    .PhysicalItemInteractionController.ReleaseCurrentItem(0f, false);
                _itemPoolManager.DeleteAndDestroyObject(item.Network);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isServer) return;

            if (other.CompareTag("Storage"))
            {
                if (Time.time - lastTransfer < transferInterval) return;
                lastTransfer = Time.time;

                if (!ResourceStorage.Storages.TryGetValue(other.gameObject, out var otherStorage))
                    return;
                if (otherStorage.StoredResources.Count == 0) return;

                Resource resourceKey = default;
                foreach (var key in otherStorage.StoredResources.Keys)
                {
                    resourceKey = key;
                    break;
                }

                otherStorage.RemoveResource(resourceKey, 1);
                storage.AddResource(resourceKey, 1);
            }
            
            else if (other.CompareTag("InteractableItem"))
            {
                if (Time.time - transferInterval < transferInterval) return;
                transferInterval = Time.time;

                if (!DropBox.Boxes.TryGetValue(other.gameObject, out var box))
                    return;
                if (box.Resources.Count == 0) return;

                Resource resourceKey = default;
                foreach (var key in box.Resources.Keys)
                {
                    resourceKey = key;
                    break;
                }

                box.Resources[resourceKey]--;
                storage.AddResource(resourceKey, 1);

                if (box.Resources[resourceKey] <= 0)
                    box.Resources.Remove(resourceKey);
            }
        }
    }
}