using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GameRandomEvents.Flood;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LmbWrench : ItemReaction
    {
        public override void Act()
        {
            Debug.Log($"Act {nameof(LmbWrench)}");
        }
    }
}