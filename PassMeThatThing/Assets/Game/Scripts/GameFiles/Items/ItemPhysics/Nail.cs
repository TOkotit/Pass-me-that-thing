using System;
using Entity;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class Nail : Projectile, IConnector
    {
        [Inject] protected PhysicalItemRegistry Registry;
        [Inject] protected DamagableRegistry DamagableRegistry;
        [SerializeField] protected int damage;
        [SerializeField] private FixedJoint connectionTo;
        [SerializeField] private ConfigurableJoint connection;
        [SerializeField] private Collider propCollider;

        private PhysicalItem _tipPiercedItem;
        private PhysicalItem _hatHitItem;
        private bool _tipInGround;
        private bool _hatInGround;

        private void Awake()
        {
            propCollider.enabled = false;
        }

        public void OnTipHit(Collider other)
        {
            if (other.CompareTag("Ground"))
            {
                _tipInGround = true;
                _tipPiercedItem = null;
                StopProjectile();
                return;
            }

            if (Registry.TryGetItem(other.gameObject, out PhysicalItem item))
            {
                _tipPiercedItem = item;
                _tipInGround = false;

                if (DamagableRegistry.TryGetDamagable(item.gameObject, out var damagable))
                {
                    if (damagable is not Furniture)
                        damagable.ServerTakeDamage(damage);
                }
            }
            else if (DamagableRegistry.TryGetDamagable(other.gameObject, out var damagable))
            {
                if (damagable is not Furniture)
                    damagable.ServerTakeDamage(damage);
            }
        }

        public void OnHatHit(Collision other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                _hatInGround = true;
                _hatHitItem = null;
                TryConnect();
                return;
            }

            if (Registry.TryGetItem(other.gameObject, out PhysicalItem item))
            {
                _hatHitItem = item;
                _hatInGround = false;
                TryConnect();
                return;
            }

            StopProjectile();
            transform.SetParent(other.transform);
        }

        public void TryConnect()
        {
            var tipReady = _tipPiercedItem || _tipInGround;
            var hatReady = _hatHitItem || _hatInGround;

            if (!tipReady || !hatReady) return;

            if (_tipPiercedItem && _hatHitItem && _tipPiercedItem != _hatHitItem)
            {
                connectionTo.connectedBody = _tipPiercedItem.Rigidbody;
                connectionTo.gameObject.SetActive(true);
                connection.connectedBody = _hatHitItem.Rigidbody;
                connection.gameObject.SetActive(true);

                if (DamagableRegistry.TryGetDamagable(_tipPiercedItem.gameObject, out var tipDamagable) && tipDamagable is Furniture tipFurniture)
                    tipFurniture.Connections.Add(this);
                if (DamagableRegistry.TryGetDamagable(_hatHitItem.gameObject, out var hatDamagable) && hatDamagable is Furniture hatFurniture)
                    hatFurniture.Connections.Add(this);

                StopProjectile();
            }
            else if (_tipInGround && _hatHitItem)
            {
                MakeStatic(_hatHitItem);
                if (DamagableRegistry.TryGetDamagable(_hatHitItem.gameObject, out var damagable) && damagable is Furniture furniture)
                    furniture.Connections.Add(this);
                StopProjectile();
            }
            else if (_tipPiercedItem && _hatInGround)
            {
                MakeStatic(_tipPiercedItem);
                if (DamagableRegistry.TryGetDamagable(_tipPiercedItem.gameObject, out var damagable) && damagable is Furniture furniture)
                    furniture.Connections.Add(this);
                StopProjectile();
            }
            else if (_tipPiercedItem && _hatHitItem && _tipPiercedItem == _hatHitItem)
            {
                StopProjectile();
            }
        }

        public void Disconnect()
        {
            if (connectionTo) connectionTo.connectedBody = null;
            if (connection) connection.connectedBody = null;

            if (_tipPiercedItem && DamagableRegistry.TryGetDamagable(_tipPiercedItem.gameObject, out var tipDamagable) && tipDamagable is Furniture tipFurniture)
                tipFurniture.Connections.Remove(this);
            if (_hatHitItem && DamagableRegistry.TryGetDamagable(_hatHitItem.gameObject, out var hatDamagable) && hatDamagable is Furniture hatFurniture)
                hatFurniture.Connections.Remove(this);
            transform.SetParent(null);
            
            _tipPiercedItem = null;
            _hatHitItem = null;
            propCollider.enabled = true;
            rb.isKinematic = true;
            rb.useGravity = true;
            DestroyAfterDelay(2f);
        }

        private void MakeStatic(PhysicalItem item)
        {
            if (!item) return;
            if (DamagableRegistry.TryGetDamagable(item.gameObject, out var damagable))
            {
                if (damagable is Furniture furniture)
                    furniture.TryConnectTo();
            }
        }
    }
}