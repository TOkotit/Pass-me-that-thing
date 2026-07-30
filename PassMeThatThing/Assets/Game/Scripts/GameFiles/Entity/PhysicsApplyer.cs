using System.Collections.Generic;
using Entity;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Game.Scripts.Systems;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity
{
    public class PhysicsApplyer
    {
        [Inject] PhysicalItemRegistry physicalItemRegistry;
        [Inject] DamagableRegistry damagableRegistry;
        [Inject] DamageSystem damageSystem;

        private static readonly AnimationCurve ForceFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.4f);
        
        public void ShotRaycast(Vector3 position, Vector3 direction, float distance,
            LayerMask layer, List<string> tags = null, float force = 0f, float damage = 0f, int toughDamage = 0)
        {
            var ray = new Ray(position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, distance, layer))
            {
                if (tags != null && !tags.Contains( hit.collider.tag) ) return;
                if (IsTargetAnItem(hit.collider.gameObject, out var item))
                {
                    item.Rigidbody.AddForceAtPosition(force * direction, hit.point, ForceMode.Impulse);
                }

                if (IsTargetDamagable(hit.collider.gameObject, out var damagable))
                {
                    if (damagable is ToughnessDamagable toughnessDamagable)
                    {
                        toughnessDamagable.Hit(hit.point, force * direction);
                    }
                    damageSystem.TakeDamage(damage, damagable, toughnessDamage : toughDamage);
                }
            }
        }

        public void ApplyForceInRadius(Vector3 position, float radius,
            List<string> tags, float force= 0f, float damage= 0f, int toughDamage = 0, AnimationCurve forceFalloff = null)
        {
            var curve = forceFalloff ?? ForceFalloff;
            foreach (var item in physicalItemRegistry.GetItems())
            {
                var distance = Vector3.Distance(position, item.transform.position);
                if (distance > radius) continue;
                var t = Mathf.Clamp01(distance / radius);
                var multiplier = curve.Evaluate(t);
                var finalForce = force * multiplier;
                var direction = (item.transform.position - position).normalized;
                item.Rigidbody.AddForce(finalForce * direction, ForceMode.Force);
                
            }
            foreach (var damagable in damagableRegistry.GetDamageables())
            {
                var distance = Vector3.Distance(position, damagable.transform.position);
                if (distance > radius) continue;
                if (damagable is ToughnessDamagable toughnessDamagable && toughnessDamagable.RagdollHandler)
                {
                    var bones = toughnessDamagable.RagdollHandler.GetRigidbodies();
                    foreach (var rb in bones)
                    {
                        var boneDist = Vector3.Distance(position, rb.transform.position);
                        var t = Mathf.Clamp01(boneDist / radius);
                        var multiplier = curve.Evaluate(t);
                        var finalForce = force * multiplier;
                        var direction = (rb.transform.position - position).normalized;
                        rb.AddForce(finalForce * direction, ForceMode.Force);
                    }
                } 
                var dist = Mathf.Clamp01(distance / radius);
                var finalDamage = curve.Evaluate(dist);
                damageSystem.TakeDamage(finalDamage, damagable, toughnessDamage : toughDamage);
            }
        }
        
        private PhysicalItem IsTargetAnItem(GameObject target, out PhysicalItem itemToReturn)
        {
            itemToReturn = physicalItemRegistry.TryGetItem(target, out var item);
            return item;
        }

        private Damagable IsTargetDamagable(GameObject target, out Damagable damagableToReturn)
        {
            damagableToReturn = damagableRegistry.TryGetDamagable(target, out var damagable);
            return damagable;
        }
    }
}