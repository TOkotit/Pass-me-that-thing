using System.Collections.Generic;
using Game.Scripts.GameFiles.Items;
using UnityEngine;
using Mirror;
using VContainer;

public class PlayerInventory : NetworkBehaviour
{
    public readonly SyncDictionary<int, ItemSlot> ServerInventory = new();
    private int size = 3;
    [Inject] PlayerInventoryModel _playerInventoryModel;
    [Inject] private ItemDatabase itemDatabase;
    
    
    public override void OnStartClient()
    {
        ServerInventory.OnChange += OnInventoryChanged;

        RefreshLocalModel();
    }
    
    private void OnInventoryChanged(SyncDictionary<int, ItemSlot>.Operation op, int index, ItemSlot newItem)
    {
        if (!isLocalPlayer) return;

        switch (op)
        {
            case SyncDictionary<int, ItemSlot>.Operation.OP_ADD:
                _playerInventoryModel.Inventory[index] = newItem;
                break;
            case SyncDictionary<int, ItemSlot>.Operation.OP_REMOVE:
                _playerInventoryModel.Inventory.Remove(index);
                break;
            case SyncDictionary<int, ItemSlot>.Operation.OP_SET:
                _playerInventoryModel.Inventory[index] = newItem;
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
    public void CmdPickUpItem(GameObject itemObject)
    {
        if (itemObject == null) return;

        var networkItem = itemObject.GetComponent<NetworkItem>();
        if (networkItem == null) return;
        
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
        
        NetworkServer.UnSpawn(itemObject);
    }
    
    [Command]
    public void CmdDropItem(int index)
    {
        if (!ServerInventory.TryGetValue(index, out var value)) return;

        var data = itemDatabase.GetItem(value.itemId);
        
        var dropped = Instantiate(data.WorldPrefab,
            transform.position + transform.forward, Quaternion.identity);
        
        NetworkServer.Spawn(dropped);
        
        ServerInventory.Remove(index);
    }
}