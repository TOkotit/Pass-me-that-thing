using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public interface IEffectController
    {
        public Vector3 ActivateEffect(Vector3 origin, Vector3 direction);
    }
}