using System;
using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;

using VContainer;

public class PlayerInventory : NetworkBehaviour
{
    public readonly SyncDictionary<int, ItemSlot> ServerInventory = new();
    private int size = 3;
    [Inject] PlayerInventoryModel _playerInventoryModel;
    [Inject] private ItemDatabase itemDatabase;
    private ItemPoolManager _itemPoolManager;
    private PhysicalItemRegistry _physicalItemRegistry;

    [SerializeField] private Transform _interactionZone;
    [SerializeField] private PhysicalItemInteractionController _physicalСontroller;
    
    // [SyncVar(OnChange = nameof(OnActiveSlotChanged))]
    public readonly SyncVar<int> activeSlot = new();
    
    [Inject]
    private void Construct(NetworkManager networkManager, PhysicalItemRegistry physicalItemRegistry)
    {
        _itemPoolManager = networkManager.GetComponent<ItemPoolManager>();
        _physicalItemRegistry = physicalItemRegistry;
    }

    private void Awake()
    {
        activeSlot.OnChange += OnActiveSlotChanged;
    }

    private void OnDestroy()
    {
        activeSlot.OnChange -= OnActiveSlotChanged;
    }
    private void OnActiveSlotChanged(int oldIndex, int newIndex, bool asServer)
    {
        if (IsOwner)
        {
            _playerInventoryModel.ActiveSlotIndex = newIndex;
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner) return;
        ServerInventory.OnChange += OnInventoryChanged;
         
        RefreshLocalModel();
    }

    public override void OnStopClient()
    {
        if (!IsOwner) return;
        
        ServerInventory.OnChange -= OnInventoryChanged;
    }

    // private void OnInventoryChanged(SyncDictionary<int, ItemSlot>.Operation op, int index, ItemSlot newItem)
    // {
    //     if (!IsOwner) return;
    //
    //     switch (op)
    //     {
    //         case SyncDictionary<int, ItemSlot>.Operation.OP_ADD:
    //         case SyncDictionary<int, ItemSlot>.Operation.OP_SET:
    //             _playerInventoryModel.Inventory[index] = newItem;
    //             break;
    //
    //         case SyncDictionary<int, ItemSlot>.Operation.OP_REMOVE:
    //             _playerInventoryModel.Inventory.Remove(index);
    //             break;
    //     }
    // }
    private void OnInventoryChanged(SyncDictionaryOperation op, int index, ItemSlot newItem, bool asServer)
    {
        if (!IsOwner) return;

        switch (op)
        {
            case SyncDictionaryOperation.Add:
            case SyncDictionaryOperation.Set:
                _playerInventoryModel.Inventory[index] = newItem;
                break;

            case SyncDictionaryOperation.Remove:
                _playerInventoryModel.Inventory.Remove(index);
                break;
                
            case SyncDictionaryOperation.Clear:
                _playerInventoryModel.Inventory.Clear();
                break;
        }
    }

    private void RefreshLocalModel()
    {
        _playerInventoryModel.Inventory.Clear();
        foreach (var item in ServerInventory)
        {
            _playerInventoryModel.Inventory.Add(item);
        }
        _playerInventoryModel.ActiveSlotIndex = activeSlot.Value; 
    }
    [ServerRpc]
    public void CmdPickUpItem(PhysicalItem physicalItem, int preferredSlot)
    {
        physicalItem.ConnectionToClient = Owner;
        TryPickUpItemInternal(physicalItem, preferredSlot);
    }

    [ServerRpc]
    public void CmdHideItem()
    {
        if (_physicalСontroller.CurrentHeldItem)
        {
            ServerManager.Despawn(_physicalСontroller.CurrentHeldItem.NetworkObject, DespawnType.Pool);
            
            _itemPoolManager.ReturnToPool(_physicalСontroller.CurrentHeldItem.NetworkObject);
        }
        _physicalСontroller.ServerClearHeldItem();
    }
    

    [ServerRpc]
    public void CmdDrawItem(int index, Vector3 pointToSpawn)
    {
        if (_physicalСontroller.CurrentHeldItem)
        {
            ServerManager.Despawn(_physicalСontroller.CurrentHeldItem.NetworkObject, DespawnType.Pool);
            
            _itemPoolManager.ReturnToPool(_physicalСontroller.CurrentHeldItem.NetworkObject);
        }
        _physicalСontroller.ServerClearHeldItem();

        if (!ServerInventory.TryGetValue(index, out var value)) return;
        
        var itemToDrop = _itemPoolManager.GetFromPool(value.itemId, pointToSpawn, Quaternion.identity);

        ServerManager.Spawn(itemToDrop);
        
        var physicalItem = _physicalItemRegistry.GetItem(itemToDrop.gameObject);
        if (!physicalItem) {Debug.LogError("КУДА-ТО ДЕЛСЯ ПРЕДМЕТ");}
        if (physicalItem)
        {
            _physicalСontroller.PhysicalPickUpItem(physicalItem);
            activeSlot.Value = index;   
            physicalItem.ConnectionToClient = Owner;
        }
    }

    [ServerRpc]
    public void CmdDropItem(int index, float throwForce, bool canThrow)
    {
        var heldItem = _physicalСontroller.CurrentHeldItem;
        if (heldItem && ServerInventory.TryGetValue(index, out var slot) && slot.itemId == heldItem.Network.itemId.Value)
        {
            Vector3 dropPos = heldItem.transform.position;
            Quaternion dropRot = heldItem.transform.rotation;
            _physicalСontroller.TargetSyncPositionForDrop(Owner, dropPos, dropRot);
            _physicalСontroller.ReleaseCurrentItem(throwForce, canThrow); 
        }
        ServerInventory.Remove(index);
    }
    [ServerRpc]
    public void CmdGiveItemToPlayer(MainCharacter target)
    {
        if (!target) return;

        var item = _physicalСontroller.CurrentHeldItem;
        if (!item) return;

        var targetController = target.MainCharacterModel.PlayerInteraction.PhysicalItemInteractionController;
        if (!targetController || targetController.CurrentHeldItem)
        {
            return;
        }

        int mySlot = -1;
        foreach (var kvp in ServerInventory)
        {
            if (kvp.Value.itemId == item.Network.itemId.Value)
            {
                mySlot = kvp.Key;
                break;
            }
        }
        if (mySlot == -1) return;

        var targetInventory = target.MainCharacterModel.PlayerInventory;
        if (!targetInventory) return;

        int targetSlot = -1;
        for (int i = 0; i < 3; i++)
        {
            if (!targetInventory.ServerInventory.ContainsKey(i))
            {
                targetSlot = i;
                break;
            }
        }
        if (targetSlot == -1) return;

        ServerInventory.Remove(mySlot);
        _physicalСontroller.ReleaseCurrentItem(0f, false);   

        targetInventory.ServerInventory[targetSlot] = new ItemSlot
        {
            itemId = item.Network.itemId.Value,
            amount = 1
        };

        targetController.PhysicalPickUpItem(item);
    }
    
    [Server]
    public void ServerPickUpItem(PhysicalItem physicalItem, int preferredSlot)
    {
        TryPickUpItemInternal(physicalItem, preferredSlot);
    }
    
    private void TryPickUpItemInternal(PhysicalItem physicalItem, int preferredSlot)
    {
        if (!physicalItem) return;
        var networkItem = physicalItem.Network;
        if (!networkItem) return;

        if (physicalItem.CanBeOwned && physicalItem.Owner)
        {
            var oldOwner = physicalItem.Owner;
            var oldInventory = oldOwner.MainCharacterModel.PlayerInventory;
            var oldController = oldOwner.MainCharacterModel.PlayerInteraction.PhysicalItemInteractionController;

            if (oldInventory)
            {
                int slotToRemove = -1;
                foreach (var kvp in oldInventory.ServerInventory)
                {
                    if (kvp.Value.itemId == networkItem.itemId.Value)
                    {
                        slotToRemove = kvp.Key;
                        break;
                    }
                }
                if (slotToRemove != -1)
                    oldInventory.ServerInventory.Remove(slotToRemove);
            }

            if (oldController)
            {
                oldController.ReleaseCurrentItem(0f, false);
            }
        }

        int targetSlot = -1;
        if (preferredSlot >= 0 && preferredSlot < size && !ServerInventory.ContainsKey(preferredSlot))
            targetSlot = preferredSlot;
        else
        {
            for (int i = 0; i < size; i++)
            {
                if (!ServerInventory.ContainsKey(i))
                {
                    targetSlot = i;
                    break;
                }
            }
        }

        if (targetSlot == -1) return; 

        ServerInventory[targetSlot] = new ItemSlot { itemId = networkItem.itemId.Value, amount = 1 };

        if (_physicalСontroller.CurrentHeldItem)
        {
            ServerManager.Despawn(_physicalСontroller.CurrentHeldItem.NetworkObject, DespawnType.Pool);
            
            _itemPoolManager.ReturnToPool(_physicalСontroller.CurrentHeldItem.NetworkObject);
            
            _physicalСontroller.ReleaseCurrentItem(0f, false);
        }

        _physicalСontroller.PhysicalPickUpItem(physicalItem);
        activeSlot.Value = targetSlot;
    }
}
