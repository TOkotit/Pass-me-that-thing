using Game.Scripts.GameFiles.Items;
using UnityEngine;
using Mirror;

public class PlayerInventory : NetworkBehaviour
{
    // Список предметов, который автоматически синхронизируется с клиентами
    public SyncList<ItemSlot> inventory = new () {new ItemSlot() {itemId = "ball", amount = 1}};

    [Command]
    public void CmdAddItem(string id) 
    {
        inventory.Add(new ItemSlot { itemId = id, amount = 1 });
    }
    
    [Command]
    public void CmdPickUpItem(GameObject itemObject)
    {
        // 1. Проверка безопасности на сервере
        if (itemObject == null) return;

        var networkItem = itemObject.GetComponent<NetworkItem>();
        if (networkItem == null) return;

        // 2. Добавляем данные в SyncList
        inventory.Add(new ItemSlot { itemId = networkItem.itemId, amount = 1 });
        
        NetworkServer.UnSpawn(itemObject);
    }
    
    [Command]
    public void CmdDropItem(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        var data = Database.GetItem(inventory[index].itemId);
        
        var dropped = Instantiate(data.WorldPrefab,
            transform.position + transform.forward, Quaternion.identity);
        
        NetworkServer.Spawn(dropped);
        
        inventory.RemoveAt(index);
    }
}