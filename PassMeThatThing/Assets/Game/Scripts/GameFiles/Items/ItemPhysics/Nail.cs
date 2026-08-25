using System;
using Entity;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using DI;

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

            if (Registry != null && Registry.TryGetItem(other.gameObject, out var item))
            {
                _tipPiercedItem = item;
                _tipInGround = false;

                if (DamagableRegistry != null && DamagableRegistry.TryGetDamagable(item.gameObject, out var damagable))
                {
                    if (damagable is not Furniture)
                        damagable.ServerTakeDamage(damage);
                }
            }
            else if (DamagableRegistry != null && DamagableRegistry.TryGetDamagable(other.gameObject, out var damagable))
            {
                if (damagable is not Furniture)
                    damagable.ServerTakeDamage(damage);
            }
        }

        public void OnHatHit(Collider other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                _hatInGround = true;
                _hatHitItem = null;
                TryConnect();
                return;
            }

            if (Registry != null && Registry.TryGetItem(other.gameObject, out var item))
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
                _tipPiercedItem.Connections.Add(this);
                _hatHitItem.Connections.Add(this);
                
                StopProjectile();
            }
            else if (_tipInGround && _hatHitItem)
            {
                MakeStatic(_hatHitItem);
                _hatHitItem.Connections.Add(this);
                StopProjectile();
            }
            else if (_tipPiercedItem && _hatInGround)
            {
                MakeStatic(_tipPiercedItem);
                _tipPiercedItem.Connections.Add(this);
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

            if (_tipPiercedItem) _tipPiercedItem.Connections.Remove(this);
            if (_hatHitItem) _hatHitItem.Connections.Remove(this);
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
            if (DamagableRegistry != null && DamagableRegistry.TryGetDamagable(item.gameObject, out var damagable))
            {
                if (damagable is Furniture furniture)
                    furniture.TryConnectTo();
            }
        }
    }
}