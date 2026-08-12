using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.Flood;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LmbWrench : ItemReaction
    {
        
        public override void Act()
        {
            Debug.Log($"Act {nameof(LmbWrench)}");
        }

        public void OnCollisionEnter(Collision other)
        {
            var otherCollider = other.collider;
            if (EventTerminalsRegistry.Instance.TryGetItem(otherCollider.gameObject, out var terminal))
            {
                Debug.Log($"<color=orange>Collision Enter {nameof(LmbWrench)}");
                if (terminal is PumpInteractTerminal or ValveInteractTerminal && _item.Owner)
                {
                    Debug.Log("<color=green> Interacting</color>");
                    terminal.TerminalAct(Item.ConnectionToClient);
                }
            }
        }
    }
}