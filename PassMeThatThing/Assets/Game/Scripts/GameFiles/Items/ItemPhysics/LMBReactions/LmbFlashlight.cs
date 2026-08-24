
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.Blackout;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LmbFlashlight : ItemReaction
    {
        public override void Act()
        {
            Debug.Log($"Act {nameof(LmbFlashlight)}");
        }

        public void OnCollisionEnter(Collision other)
        {



            if (EventTerminalsRegistry.Instance.TryGetItem(other.gameObject, out var terminal))
            {
                Debug.Log($"<color=orange>Collision Enter {nameof(LmbFlashlight)}");
                if (terminal is BlackoutBlowFuseTerminal && _item.Owner) 
                {
                    Debug.Log("<color=green> Interacting</color>");
                    terminal.TerminalAct(_item.ConnectionToClient);
                }
            }
        }
    }
}