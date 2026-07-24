using System;
using System.Collections;
using AYellowpaper.SerializedCollections;
using Enums;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.Systems;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Turrets
{
    public class TurretAttackController : MonoBehaviour
    {
        [Inject] private DamageSystem _damageSystem;
        [SerializedDictionary] public SerializedDictionary<DamagableType, float> damageTypes;
        
        [SerializeField] private LineRenderer lineRenderer;

        private Coroutine _endDrawRayCoroutine;

        private void Awake()
        {
            lineRenderer.positionCount = 2;
        }

        public void AttackRay(float damage, TargetObject target)
        {
            lineRenderer.enabled = true;
            
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.transform.position);
            
            if (_endDrawRayCoroutine != null) StopCoroutine(_endDrawRayCoroutine);
            StartCoroutine(EndDrawRayCoroutine());
            
            _damageSystem.TakeDamage(damage, target.gameObject, damageTypes,gameObject);
        }

        private IEnumerator EndDrawRayCoroutine()
        {
            yield return new WaitForSeconds(1f);
            lineRenderer.enabled = false;
        }
    }
}