using Game.Scripts.GameFiles.LevelGeneration.Graph;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    private List<ItemData> _taskItems = new List<ItemData>();

    public List<ItemData> allItems;

    public List<ItemData> TaskItems
    {
        get
        {
            if (_taskItems.Count == 0)
            {
                _taskItems = allItems.Where(x => x.CanAppearInTask).ToList();
            }
            return _taskItems;
        }
    }

    public ItemData GetItem(string id)
    {
        return allItems.Find(item => item.Id == id);
    }
}
