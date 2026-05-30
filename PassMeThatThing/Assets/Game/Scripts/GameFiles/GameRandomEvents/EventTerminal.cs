using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class EventTerminal : NetworkBehaviour
    {
        [Inject] protected EventTerminalsRegistry Registry { get; private set; }
        
        public virtual void TerminalAct() {}


        public override void OnStartClient()
        {
            base.OnStartClient();
            Registry = EventTerminalsRegistry.Instance;
            Registry.Register(this); 
        }

        protected virtual void OnDestroy()
        {
            Registry.Unregister(this); 
        } 
    }
}