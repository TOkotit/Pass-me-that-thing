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
        [SerializeField] private float collisionDamageThreshold = 5f;      // минимальная скорость удара для получения урона
        [SerializeField] private float collisionDamageMultiplier = 1f;     // множитель урона от превышения порога
        [SerializeField] private float flatDamageReduction = 0f;           // плоское снижение урона (для прочных предметов)

        [SerializeField] private PhysicalItem _item;
        [SerializeField] private List<PhysicalItem> _items;
        private readonly List<Collider> _connectedTo = new List<Collider>();
        private string _savedTag = "Item";
        public PhysicalItem Item => _item;

        public override void OnDeath()
        {
            RagdollHandler.EnableRagdoll();
            foreach (var item in _items)
            {
                item.gameObject.SetActive(true);
                item.transform.SetParent(null);
            }
            if (_item.gameObject != gameObject) Destroy(_item.gameObject);
            Destroy(gameObject);
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
        }

        public override void OnToughnessBreak()
        {
            RagdollHandler.EnableRagdoll();
            tag = _savedTag;
            foreach (var connection in _item.Connections)
            {
                connection.Disconnect();
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
                RagdollHandler.DisableRagdoll();
            }
        }
    }
}