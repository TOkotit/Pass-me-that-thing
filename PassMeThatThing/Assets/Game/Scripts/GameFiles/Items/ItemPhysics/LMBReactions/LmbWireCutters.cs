using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.Blackout;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LmbWireCutters : ItemReaction
    {
        public override void Act()
        {
            Debug.Log($"Act {nameof(LmbWireCutters)}");
        }

        public void OnCollisionEnter(Collision other)
        {
            if (EventTerminalsRegistry.Instance.TryGetItem(other.gameObject, out var terminal))
            {
                Debug.Log($"<color=orange>Collision Enter {nameof(LmbWireCutters)}");
                
                if (terminal is BlackoutCutWiresTerminal && _item.Owner) 
                {
                    Debug.Log("<color=green> Interacting</color>");
                    terminal.TerminalAct(_item.ConnectionToClient);
                }
            }
        }
    }
}