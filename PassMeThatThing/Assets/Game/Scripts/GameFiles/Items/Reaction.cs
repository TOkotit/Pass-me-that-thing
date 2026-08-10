using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public abstract class Reaction : NetworkBehaviour, IReaction
    {
        public abstract void Act();
    }
}