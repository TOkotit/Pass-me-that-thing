using System;
using System.Collections.Generic;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;
using Mirror;
using Mirror.Examples.RigidbodyPhysics;
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
    [SyncVar(hook = nameof(OnActiveSlotChanged))]
    public int activeSlot;
    
    [Inject]
    private void Construct(NetworkManager networkManager, PhysicalItemRegistry physicalItemRegistry)
    {
        _itemPoolManager = networkManager.GetComponent<ItemPoolManager>();
        _physicalItemRegistry = physicalItemRegistry;
    }
    

    private void OnActiveSlotChanged(int oldIndex, int newIndex)
    {
        if (isLocalPlayer)
        {
            _playerInventoryModel.ActiveSlotIndex = newIndex;
        }
    }
    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;
        base.OnStartClient();
        ServerInventory.OnChange += OnInventoryChanged;
        _playerInventoryModel.ActiveSlotIndex = activeSlot;  
        RefreshLocalModel();
    }

    public override void OnStopClient()
    {
        if (isLocalPlayer)
            ServerInventory.OnChange -= OnInventoryChanged;
    }

    private void OnInventoryChanged(SyncDictionary<int, ItemSlot>.Operation op, int index, ItemSlot newItem)
    {
        if (!isLocalPlayer) return;

        switch (op)
        {
            case SyncDictionary<int, ItemSlot>.Operation.OP_ADD:
            case SyncDictionary<int, ItemSlot>.Operation.OP_SET:
                _playerInventoryModel.Inventory[index] = newItem;
                break;

            case SyncDictionary<int, ItemSlot>.Operation.OP_REMOVE:
                _playerInventoryModel.Inventory.Remove(index);
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
    }
    [Command]
    public void CmdPickUpItem(PhysicalItem physicalItem, int preferredSlot)
    {
        physicalItem.ConnectionToClient = connectionToClient;
        TryPickUpItemInternal(physicalItem, preferredSlot);
    }

    [Command]
    public void CmdDrawItem(int index, Vector3 pointToSpawn)
    {
        if (_physicalСontroller.CurrentHeldItem)
        {
            NetworkServer.UnSpawn(_physicalСontroller.CurrentHeldItem.gameObject);
        }
        _physicalСontroller.ServerClearHeldItem();

        if (!ServerInventory.TryGetValue(index, out var value)) return;
        var itemToDrop = _itemPoolManager.GetFromPool(value.itemId);

        itemToDrop.transform.position = pointToSpawn;
        NetworkServer.Spawn(itemToDrop, connectionToClient);
        
        itemToDrop.SetActive(true);
        var physicalItem = _physicalItemRegistry.GetItem(itemToDrop.gameObject);
        if (!physicalItem) {Debug.LogError("КУДА-ТО ДЕЛСЯ ПРЕДМЕТ");}
        if (physicalItem)
        {
            _physicalСontroller.PhysicalPickUpItem(physicalItem);
            activeSlot = index;   
        }
    }

    [Command]
    public void CmdDropItem(int index, float throwForce, bool canThrow)
    {
        var heldItem = _physicalСontroller.CurrentHeldItem;
        if (heldItem && ServerInventory.TryGetValue(index, out var slot) && slot.itemId == heldItem.Network.itemId)
        {
            Vector3 dropPos = heldItem.transform.position;
            Quaternion dropRot = heldItem.transform.rotation;
            _physicalСontroller.TargetSyncPositionForDrop(connectionToClient, dropPos, dropRot);
            _physicalСontroller.ReleaseCurrentItem(throwForce, canThrow); 
        }
        ServerInventory.Remove(index);
    }
    [Command]
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
            if (kvp.Value.itemId == item.Network.itemId)
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
            itemId = item.Network.itemId,
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
                    if (kvp.Value.itemId == networkItem.itemId)
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

        ServerInventory[targetSlot] = new ItemSlot { itemId = networkItem.itemId, amount = 1 };

        if (_physicalСontroller.CurrentHeldItem)
        {
            NetworkServer.UnSpawn(_physicalСontroller.CurrentHeldItem.gameObject);
            _physicalСontroller.ReleaseCurrentItem(0f, false);
        }

        _physicalСontroller.PhysicalPickUpItem(physicalItem);
        activeSlot = targetSlot;
    }
}
