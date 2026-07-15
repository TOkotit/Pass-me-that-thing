using System;
using System.Linq;
using Game.Scripts.Enums;
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
        private float lastTransfer;
        private float transferInterval;
        public void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            Debug.Log("В приемник что-то попало");
            if (!other.CompareTag("Item")) return;

            if (!registry.TryGetItem(other.gameObject, out var item)) return;

            if (item.Resources.Count > 0)
            {
                if (item.Owner)
                {
                    var playerInventory = item.Owner.MainCharacterModel.PlayerInventory;
                    if (playerInventory)
                    {
                        var slotToRemove = -1;
                        foreach (var kvp in playerInventory.ServerInventory)
                        {
                            if (kvp.Value.itemId == item.Network.itemId)
                            {
                                slotToRemove = kvp.Key;
                                break;
                            }
                        }
                        if (slotToRemove != -1)
                            playerInventory.ServerInventory.Remove(slotToRemove);
                    }
                }

                foreach (var resourcePair in item.Resources)
                {
                    storage.AddResource(resourcePair.Key, resourcePair.Value);
                }

                item.Owner.MainCharacterModel.PlayerInteraction.PhysicalItemInteractionController.ReleaseCurrentItem(0f, false);
                NetworkServer.UnSpawn(other.gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isServer) return;                     
            if (!other.CompareTag("Storage")) return;
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
    }
}