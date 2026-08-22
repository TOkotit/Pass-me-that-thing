using System;
using System.Collections.Generic;
using Entity;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class Furniture : ToughnessDamagable
    {
        [Inject] protected DamagableModel _model;
        public override DamagableModel DamagableModel => _model;

        [Header("Collision Damage")]
        [SerializeField] private float collisionDamageThreshold = 5f;
        [SerializeField] private float collisionDamageMultiplier = 1f;
        [SerializeField] private float flatDamageReduction = 0f;

        [SerializeField] private PhysicalItem _item;
        [SerializeField] private List<PhysicalItem> _items = new List<PhysicalItem>();
        private readonly List<Collider> _connectedTo = new List<Collider>();
        private string _savedTag = "Item";

        public PhysicalItem Item => _item;

        public override void OnDeath()
        {
            RagdollHandler?.EnableRagdoll();

            foreach (var item in _items)
            {
                if (item)
                {
                    item.gameObject.SetActive(true);
                    item.transform.SetParent(null);
                }
            }
            _items.Clear();

            if (_item && _item.gameObject != gameObject)
                NetworkServer.Destroy(_item.gameObject);

            NetworkServer.Destroy(gameObject);
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
        }

        public override void OnToughnessBreak()
        {
            RagdollHandler?.EnableRagdoll();
            tag = _savedTag;

            if (_item && _item.Connections != null)
            {
                foreach (var connection in _item.Connections)
                {
                    connection?.Disconnect();
                }
            }
        }

        public override void OnToughnessChanged(int currentToughness, int maxToughness)
        {
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isServer) return;

            if (other.gameObject.CompareTag("Ground"))
            {
                _connectedTo.Add(other.collider);
            }

            var impactSpeed = other.relativeVelocity.magnitude;
            if (impactSpeed > collisionDamageThreshold)
            {
                var damage = (impactSpeed - collisionDamageThreshold) * collisionDamageMultiplier - flatDamageReduction;
                damage = Mathf.Max(0f, damage);
                if (damage > 0f)
                {
                    ServerTakeDamage((int)damage);
                }
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                _connectedTo.Remove(other.collider);
            }
        }

        public void TryConnectTo()
        {
            if (_connectedTo.Count > 0)
            {
                _savedTag = tag;
                tag = "Ground";
                RagdollHandler?.DisableRagdoll();
            }
        }
    }
}