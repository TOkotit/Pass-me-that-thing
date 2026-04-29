using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public static class Database
    {
        //TODO перенести в контейнер
        public static ItemDatabase _instance { get; private set; }

        public static ItemData GetItem(string id)
        {
            if (_instance == null)
                _instance = Resources.Load<ItemDatabase>("Items/MainItemDatabase");
            
            return _instance.GetItem(id);
        }
    }
}