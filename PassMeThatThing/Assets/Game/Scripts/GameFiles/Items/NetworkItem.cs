using FishNet.Object;
using FishNet.Object.Synchronizing;


namespace Game.Scripts.GameFiles.Items
{
    public class NetworkItem : NetworkBehaviour 
    {
        // [SyncVar] 
        public readonly SyncVar<string> itemId = new();
        
        
    }
}