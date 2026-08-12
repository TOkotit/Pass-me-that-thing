using System;
using Entity;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class Nail : Projectile
    {
        [Inject] protected PhysicalItemRegistry Registry;
        [Inject] protected DamagableRegistry DamagableRegistry;
        protected Collider lastHitItem;
        public void OnTipHit(Collider other)
        {
            if (other.CompareTag("Ground"))
            {
                StopProjectile();
                DamagableRegistry.TryGetDamagable(lastHitItem.gameObject, out var damagable);
                if (damagable is Furniture furniture)
                {
                    furniture.TryConnectTo();
                }
            }

            if (Registry.TryGetItem(other.gameObject, out PhysicalItem item))
            {
                lastHitItem = other;
            }
        }

        public void OnHatHit(Collider other)
        {
            StopProjectile();
            transform.SetParent(other.transform);
        }
    }
}