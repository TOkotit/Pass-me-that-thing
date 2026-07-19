using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class GlobalInventoryManager : NetworkBehaviour
    {
        public Dictionary<NetworkConnectionToClient, PlayerInventory> allInventories = new();

        [Server]
        public void AddInventory(NetworkConnectionToClient connection, PlayerInventory inventory)
        {
            allInventories[connection] = inventory;
            Debug.Log($"[GlInvManager] Inventory Added: {connection.connectionId}");
        }

        [Command(requiresAuthority =  false)]
        public void CmdDeleteFromInventory(string instanceId, NetworkConnectionToClient sender = null)
        {
            Debug.Log($"[GlInvManager] CmdDeleteFromInventory: {sender?.connectionId}");
            DeleteFromInventory(instanceId, sender);
        }

        [Server]
        public void DeleteFromInventory(string instanceId, NetworkConnectionToClient connection)
        {
            allInventories[connection].ServerDeleteItem(instanceId);
        }
        
        
        
    }
}