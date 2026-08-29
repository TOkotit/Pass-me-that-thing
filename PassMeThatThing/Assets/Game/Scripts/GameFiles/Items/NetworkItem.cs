using Mirror;
using System;

namespace Game.Scripts.GameFiles.Items
{
    public class NetworkItem : NetworkBehaviour 
    {
        //TODO Итемдата не синхронизируется и на клиенте нул
        //Нужно получать дату по айди через базу
        [SyncVar] public string itemId;
        [SyncVar] public string instanceId;
        private ItemData itemData;
        [Obsolete]
        public ItemData ItemData {get => itemData; set => itemData = value;}
    }
}