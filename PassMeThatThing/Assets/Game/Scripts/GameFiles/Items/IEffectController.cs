using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public abstract class EffectController : NetworkBehaviour
    {
        public abstract Vector3 ActivateEffect(Vector3 origin, Vector3 direction);
    }
}