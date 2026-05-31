using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.Events.Blackout;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBFlashlight : LMBReaction
    {
        public LMBFlashlight(PhysicalItem item) : base(item)
        {
        }

        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBFlashlight)}");
        }

        public override void CollisionEnter(Collision other)
        {
            if (EventTerminalsRegistry.Instance.TryGetItem(other.gameObject, out var terminal))
            {
                Debug.Log($"<color=orange>Collision Enter {nameof(LMBFlashlight)}");
                if (terminal is BlackoutBlowFuseTerminal) 
                {
                    Debug.Log("<color=green> Interacting</color>");
                    terminal.TerminalAct(Item.ConnectionToClient);
                }
            }
        }
    }
}