using System.Collections.Generic;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class MeleeItem : NetworkBehaviour
    {
        [Inject] PhysicsApplyer physicsApplyer;

        [SerializeField] private PhysicalItem item;
        [SerializeField] private List<AnimationClip> attackClips = new();
        [SerializeField] private float damage;
        [SerializeField] private int toughnessDamage;
        [SerializeField] private float impactMultiplier = 1f;

        private readonly HashSet<Collider> _contacts = new();

        public IReadOnlyCollection<Collider> Contacts => _contacts;
        public bool HasContacts => _contacts.Count > 0;
        public IReadOnlyList<AnimationClip> AttackClips => attackClips;

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            if (!item || !item.Rigidbody || physicsApplyer == null) return;

            _contacts.Add(other);

            var impulse = item.Rigidbody.mass * item.Rigidbody.linearVelocity * impactMultiplier;
            physicsApplyer.ApplyForceAndDamageToTarget(
                other.gameObject,
                impulse,
                damage,
                toughnessDamage,
                transform.position,
                forceMode: ForceMode.Impulse);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isServer) return;
            _contacts.Remove(other);
        }

        
        public bool HasForeignContacts(Transform exclude)
        {
            foreach (var c in _contacts)
            {
                if (!c) continue;
                if (exclude && c.transform.IsChildOf(exclude)) continue;
                return true;
            }
            return false;
        }

        public void ClearContacts() => _contacts.Clear();
    }
}