using System;
using System.Collections;
using AYellowpaper.SerializedCollections;
using Enums;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.Systems;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Turrets
{
    public class TurretAttackController : MonoBehaviour
    {
        [Inject] private DamageSystem _damageSystem;
        [SerializedDictionary] public SerializedDictionary<DamagableType, float> damageTypes;
        

        public void AttackRay(float damage, TargetObject target)
        {
            _damageSystem.TakeDamage(damage, target.Damageable, damageTypes);
        }

        
    }
}