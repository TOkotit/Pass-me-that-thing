using System;
using System.Collections.Generic;
using Entity;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class Furniture : ToughnessDamagable
    {
        public override DamagableModel DamagableModel { get; }
        [SerializeField] private PhysicalItem _item;
        [SerializeField] private List<PhysicalItem> _items;
        private List<Collider> _connectedTo = new List<Collider>();
        //private List<Collider> _connectedObjects = new List<Collider>();
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
            throw new NotImplementedException();
        }

        public override void OnToughnessBreak()
        {
            RagdollHandler.EnableRagdoll();
        }

        public override void OnToughnessChanged(int currentToughness, int maxToughness)
        {
            throw new NotImplementedException();
        }

        private void OnCollisionEnter(Collision other)
        {
            if ((other.gameObject.CompareTag("Furniture") 
                || other.gameObject.CompareTag("Ground")) 
                //&& !_connectedObjects.Contains(other.collider)
                )
            {
                _connectedTo.Add(other.collider);
            }
        }

        private void OnCollisionExit(Collision other)
        {
            _connectedTo.Remove(other.collider);
        }

        public void TryConnectTo()
        {
            if (_connectedTo.Count > 0)
            {
                RagdollHandler.DisableRagdoll();
            }
        }

        /*public void TryConnectItem(Collider collider)
        {
            _connectedObjects.Add(collider);
            _connectedTo.Remove(collider);
        }*/
    }
}