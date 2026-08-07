
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.Blackout;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBFlashlight : LMBReaction
    { 
        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBFlashlight)}");
        }

        public void OnCollisionEnter(Collision other)
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