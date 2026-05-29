using System;
using Entity;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity
{
    public class CollisionDamageDealer : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (DamagableRegistry.Instance.TryGetDamagable(other.gameObject, out var damagable))
            {
                
            }
        }
    }
}