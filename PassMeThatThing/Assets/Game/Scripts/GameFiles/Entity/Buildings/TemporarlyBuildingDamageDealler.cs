using Entity;
using Enums;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class TemporarlyBuildingDamageDealler : NetworkBehaviour
    {
        [SerializeField] private float radius = 5f;
        [SerializeField] private int damagePerSecond = 10;
        [SerializeField] private float interval = 1f;

        private float _nextDamageTime;
        private readonly Collider[] _colliderBuffer = new Collider[100]; 

        private void Update()
        {
            if (!isServer) return;

            if (Time.time >= _nextDamageTime)
            {
                _nextDamageTime = Time.time + interval;
                DealDamage();
            }
        }

        private void DealDamage()
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, _colliderBuffer);
            for (int i = 0; i < hits; i++)
            {
                var col = _colliderBuffer[i];
                if (!col) continue;

                var dam = FindDamagableInHierarchy(col.gameObject);
                if (!dam || dam.Type != DamagableType.Building) continue;

                dam.ServerTakeDamage(damagePerSecond);
            }
        }

        private Damagable FindDamagableInHierarchy(GameObject obj)
        {
            var t = obj.transform;
            while (t)
            {
                if (DamagableRegistry.Instance.TryGetDamagable(t.gameObject, out var damagable))
                    return damagable;
                t = t.parent;
            }
            return null;
        }
    }
}