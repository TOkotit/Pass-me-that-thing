using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public abstract class LMBReaction
    {
        protected PhysicalItem Item;

        public LMBReaction(PhysicalItem item)
        {
            Item = item;
        }
        public abstract void Act();
        public abstract void CollisionEnter(Collision other);
    }
}