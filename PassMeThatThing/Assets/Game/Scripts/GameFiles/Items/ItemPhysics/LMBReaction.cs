using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public abstract class LMBReaction : NetworkBehaviour
    {
        [SerializeField] protected PhysicalItem Item;

        public abstract void Act();
    }
}