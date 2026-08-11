using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public abstract class Reaction : NetworkBehaviour, IReaction
    {
        public abstract void Act();
        /// <summary> Должно ли действие выполняться непрерывно, пока зажата кнопка. </summary>
        public virtual bool IsContinuous => false;
    }
}