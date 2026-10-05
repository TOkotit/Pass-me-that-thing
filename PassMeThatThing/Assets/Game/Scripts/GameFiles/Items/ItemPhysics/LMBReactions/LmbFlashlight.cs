
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
    }
}