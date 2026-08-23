using System.Collections.Generic;
using Entity;
using Game.Entity;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public interface INetworkRagdoll
    {
        public void Setup(Damageable damageable, Dictionary<string, Rigidbody> boneDict);
        public void EnableRagdoll();
        public void DisableRagdoll();
    }
}