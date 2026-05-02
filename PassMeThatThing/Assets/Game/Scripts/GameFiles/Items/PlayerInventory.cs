using System.Collections.Generic;
using Game.Scripts.GameFiles.Items;
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
    // private MainCharacterMovement _characterMovement;

    [SerializeField] private Transform _interactionZone;
    [SerializeField] public float throwMultiplier = 2.5f;

    [Inject]
    private void Construct(NetworkManager networkManager)
    {
        _itemPoolManager = networkManager.GetComponent<ItemPoolManager>();
    }

    public override void OnStartClient()
    {
        // _characterMovement = GetComponent<MainCharacterMovement>();

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
        
        // var data = itemDatabase.GetItem(value.itemId);
        // var spawnPos = transform.position + transform.forward;
        // var dropped = Instantiate(data.WorldPrefab, spawnPos, Quaternion.identity);
        
        var itemToDrop = _itemPoolManager.GetFromPool(value.itemId);
        
        itemToDrop.transform.position = _interactionZone.position;
        itemToDrop.SetActive(true);
        
        NetworkServer.Spawn(itemToDrop);
        
        // if (itemToDrop.TryGetComponent<Rigidbody>(out var rb))
        // {
        //     var moveDir = _characterMovement.MoveDirection;
        //     
        //     var currentSpeed = 5f; 
        //     Debug.Log(_characterMovement);
        //     var finalVelocity = moveDir * (throwMultiplier * currentSpeed);
        //     
        //     if (moveDir.magnitude > 0.1f)
        //     {
        //         finalVelocity += Vector3.up * 2f;
        //     }
        //     
        //     rb.AddForce(finalVelocity, ForceMode.VelocityChange);
        // }
        
        ServerInventory.Remove(index);
    }
}