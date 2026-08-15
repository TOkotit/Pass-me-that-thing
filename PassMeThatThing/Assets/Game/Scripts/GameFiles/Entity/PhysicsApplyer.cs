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
                if (tags != null && !tags.Contains(hit.collider.tag)) return;
                
                ApplyForceAndDamageToTarget(
                    hit.collider.gameObject,
                    force * direction,          
                    damage,
                    toughDamage,
                    hit.point,
                    callHitOnToughness: true,   
                    forceMode: ForceMode.Impulse);
            }
        }

        public void ApplyForceInRadius(Vector3 position, float radius,
            List<string> tags, float force = 0f, float damage = 0f, int toughDamage = 0,
            AnimationCurve forceFalloff = null)
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

                ApplyForceAndDamageToTarget(
                    item.gameObject,
                    finalForce * direction,
                    damage * multiplier,
                    toughDamage,
                    item.transform.position,
                    callHitOnToughness: false,  
                    forceMode: ForceMode.Force);
            }

            foreach (var damagable in damagableRegistry.GetDamageables())
            {
                var distance = Vector3.Distance(position, damagable.transform.position);
                if (distance > radius) continue;

                var t = Mathf.Clamp01(distance / radius);
                var multiplier = curve.Evaluate(t);

                if (damagable is ToughnessDamagable toughnessDamagable && toughnessDamagable.RagdollHandler)
                {
                    var bones = toughnessDamagable.RagdollHandler.GetRigidbodies();
                    foreach (var rb in bones)
                    {
                        var boneDist = Vector3.Distance(position, rb.transform.position);
                        var boneT = Mathf.Clamp01(boneDist / radius);
                        var boneMultiplier = curve.Evaluate(boneT);
                        var finalBoneForce = force * boneMultiplier;
                        var boneDirection = (rb.transform.position - position).normalized;
                        rb.AddForce(finalBoneForce * boneDirection, ForceMode.Force);
                    }
                }

                ApplyDamageToTarget(
                    damagable,
                    damage * multiplier,
                    toughDamage,
                    callHitOnToughness: false);
            }
        }

        public void ApplyForceAndDamageToTarget(
            GameObject target,
            Vector3 force,
            float damage,
            int toughDamage,
            Vector3 hitPoint,
            bool callHitOnToughness = false,
            ForceMode forceMode = ForceMode.Force)
        {
            if (!target) return;

            if (physicalItemRegistry.TryGetItem(target, out var item) && item.Rigidbody != null)
            {
                if (forceMode == ForceMode.Impulse)
                    item.Rigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
                else
                    item.Rigidbody.AddForce(force, forceMode);
            }

            if (damagableRegistry.TryGetDamagable(target, out var damagable))
            {
                ApplyDamageToTarget(damagable, damage, toughDamage, callHitOnToughness, hitPoint, force);
            }
        }

        private void ApplyDamageToTarget(
            Damagable damagable,
            float damage,
            int toughDamage,
            bool callHitOnToughness,
            Vector3 hitPoint = default,
            Vector3 force = default)
        {
            if (!damagable) return;

            if (callHitOnToughness && damagable is ToughnessDamagable toughnessDamagable)
            {
                toughnessDamagable.Hit(hitPoint, force);
            }

            damageSystem.TakeDamage(damage, damagable, toughnessDamage: toughDamage);
        }
    }
}