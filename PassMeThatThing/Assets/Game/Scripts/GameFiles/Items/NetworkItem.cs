using Mirror;

namespace Game.Scripts.GameFiles.Items
{
    public class NetworkItem : NetworkBehaviour 
    {
        [SyncVar] public string itemId;
        [SyncVar] public string instanceId;
        private ItemData itemData;
        public ItemData ItemData {get => itemData; set => itemData = value;}
    }
}