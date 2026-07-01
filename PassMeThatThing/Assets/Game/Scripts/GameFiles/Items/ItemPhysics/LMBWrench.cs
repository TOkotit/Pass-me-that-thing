using DG.Tweening;
using FishNet.Object;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.GameEvents.FloodEvent;
using Game.Scripts.GameFiles.GameRandomEvents.FloodEvent;

using UnityEditor;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LMBWrench : LMBReaction
    {
        
        public LMBWrench(PhysicalItem item) : base(item)
        {
        }

        public override void Act()
        {
            Debug.Log($"Act {nameof(LMBWrench)}");
            
            
        }

        [Server]
        public override void CollisionEnter(Collision other)
        {
            var otherCollider = other.collider;
            
            if (EventTerminalsRegistry.Instance.TryGetItem(otherCollider.gameObject, out var terminal))
            {
                if (terminal is PumpInteractTerminal or ValveInteractTerminal)
                {
                    terminal.TerminalAct(Item.ConnectionToClient);
                }
            }
        }
    }
}