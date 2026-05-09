using System;
using System.Collections.Generic;
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
    
    [Inject]
    private void Construct(NetworkManager networkManager, PhysicalItemRegistry physicalItemRegistry)
    {
        _itemPoolManager = networkManager.GetComponent<ItemPoolManager>();
        _physicalItemRegistry = physicalItemRegistry;
    }

    public override void OnStartClient()
    {
        if (!isLocalPlayer)
            return;

        ServerInventory.OnChange += OnInventoryChanged;
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
    public void CmdPickUpItem(PhysicalItem physicalItem)
    {
        if (!physicalItem) return;
        var networkItem = physicalItem.Network;
        Debug.Log("Первая проверка");
        if (!networkItem) return;
        Debug.Log("Вторая проверка");
        var emptyIdx = -1;
        
        for (var i = 0 ; i < size; i++)
        {
            if (!ServerInventory.ContainsKey(i))
            {
                emptyIdx = i; break;
            }
        }
        
        if (emptyIdx == -1) return;
        
        if (emptyIdx < 0 || emptyIdx >= size) return;
        ServerInventory[emptyIdx] = new ItemSlot { itemId = networkItem.itemId, amount = 1 };
        
    }

    [Command]
    public void CmdDrawItem(int index, Vector3 pointToSpawn)
    {
        if (_physicalСontroller.CurrentHeldItem)
        {
            NetworkServer.UnSpawn(_physicalСontroller.CurrentHeldItem.gameObject);
        }
        _physicalСontroller.ClearHeldItem();
        if (!ServerInventory.TryGetValue(index, out var value)) return;
        var itemToDrop = _itemPoolManager.GetFromPool(value.itemId);
        
        itemToDrop.transform.position = pointToSpawn;
        NetworkServer.Spawn(itemToDrop);
        itemToDrop.SetActive(true);
        _physicalСontroller.SetHeldItem(_physicalItemRegistry.TryGetItem(itemToDrop.gameObject));
    }

    [Command]
    public void CmdDropItem(int index)
    {
        ServerInventory.Remove(index);
    }
}
