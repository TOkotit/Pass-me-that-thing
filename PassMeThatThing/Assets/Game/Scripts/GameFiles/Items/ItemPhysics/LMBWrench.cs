using DG.Tweening;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.GameEvents.FloodEvent;
using Game.Scripts.GameFiles.GameRandomEvents.FloodEvent;
using Mirror;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBWrench : LMBReaction
    {

        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBWrench)}");
        }

        //[Server]
        public void OnCollisionEnter(Collision other)
        {
            Debug.Log($"коснулся! {other}");
            var otherCollider = other.collider;
            if (EventTerminalsRegistry.Instance.TryGetItem(otherCollider.gameObject, out var terminal))
            {
                Debug.Log($"коснулся! {nameof(LMBWrench)}");
                if (terminal is PumpInteractTerminal or ValveInteractTerminal)
                {
                    terminal.TerminalAct(Item.ConnectionToClient);
                }
            }
        }
    }
}