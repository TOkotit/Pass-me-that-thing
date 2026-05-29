using Mirror;

namespace Game.Scripts.GameFiles.Items
{
    public class NetworkItem : NetworkBehaviour 
    {
        [SyncVar] public string itemId;
        
        
    }
}