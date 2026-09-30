using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Entity;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class Furniture : ToughnessDamageable
    {
        [Inject] protected DamagableModel _model;
        public override DamagableModel DamagableModel => _model;

        [Header("Collision Damage")]
        [SerializeField] private float collisionDamageThreshold = 5f;
        [SerializeField] private float collisionDamageMultiplier = 1f;
        [SerializeField] private float flatDamageReduction = 0f;
        [SerializeField] private int toughness;
        [SerializeField] private PhysicalItem item;
        [SerializedDictionary] public SerializedDictionary<ItemData,Transform> _itemsToSpawn;
        [Inject] ItemSpawner _itemSpawner;
        [Inject] private GlobalInventoryManager _globalInventoryManager;
        private readonly List<Collider> _connectedTo = new List<Collider>();
        private string _savedTag = "Item";
    
        public PhysicalItem Item => item;

        protected override void Awake()
        {
            base.Awake();
            ServerSetMaxToughness(toughness,true);
        }

        public override void OnDeath()
        {
            if (item && item.Network)
                _globalInventoryManager.RemoveFromAllInventories(item.Network.instanceId);

            RagdollHandler?.EnableRagdoll();

            foreach (var kvp in _itemsToSpawn)
                _itemSpawner.ServerSpawnItem(kvp.Key.Id, kvp.Value.position);
            _itemsToSpawn.Clear();

            if (item && item.gameObject != gameObject)
                NetworkServer.Destroy(item.gameObject);

            NetworkServer.Destroy(gameObject);
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
        }

        public override void OnToughnessBreak()
        {
            RagdollHandler?.EnableRagdoll();
            tag = _savedTag;

            if (item && item.Connections != null)
            {
                foreach (var connection in item.Connections)
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