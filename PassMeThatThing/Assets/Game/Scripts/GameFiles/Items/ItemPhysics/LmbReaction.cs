using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public abstract class LmbReaction : Reaction
    {
        protected PhysicalItem _item;

        public PhysicalItem Item
        {
            get => _item;
            set
            {
                if (_item == value) return;
                _item = value;
            }
        }
    }
}