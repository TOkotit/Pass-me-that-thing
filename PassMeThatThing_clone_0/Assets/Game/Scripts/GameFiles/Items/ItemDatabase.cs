using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;
    
    public ItemData GetItem(string id)
    {
        return allItems.Find(item => item.Id == id);
    }
}
