using DG.Tweening;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.GameEvents.FloodEvent;
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

        public override void CollisionEnter(Collision other)
        {
            var otherCollider = other.collider;
            // Debug.Log($"Физическое столкновение с коллайдером: {otherCollider.name}");
            
            if (EventTerminalsRegistry.Instance.TryGetItem(otherCollider.gameObject, out var terminal))
            {
                Debug.Log($"conn {Item.ConnectionToClient.connectionId}");
                terminal.TerminalAct();
            }
        }
    }
}