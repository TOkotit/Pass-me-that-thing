using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.Blackout;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBWireCutters : LMBReaction
    {
        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBWireCutters)}");
        }

        public void OnCollisionEnter(Collision other)
        {
            if (EventTerminalsRegistry.Instance.TryGetItem(other.gameObject, out var terminal))
            {
                Debug.Log($"<color=orange>Collision Enter {nameof(LMBWireCutters)}");
                
                if (terminal is BlackoutCutWiresTerminal) 
                {
                    Debug.Log("<color=green> Interacting</color>");
                    terminal.TerminalAct(Item.ConnectionToClient);
                }
            }
        }
    }
}