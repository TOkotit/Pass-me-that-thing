using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.Flood;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBWrench : LMBReaction
    {

        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBWrench)}");
        }

        public void OnCollisionEnter(Collision other)
        {
            var otherCollider = other.collider;
            if (EventTerminalsRegistry.Instance.TryGetItem(otherCollider.gameObject, out var terminal))
            {
                Debug.Log($"<color=orange>Collision Enter {nameof(LMBWrench)}");
                if (terminal is PumpInteractTerminal or ValveInteractTerminal)
                {
                    Debug.Log("<color=green> Interacting</color>");
                    terminal.TerminalAct(Item.ConnectionToClient);
                }
            }
        }
    }
}