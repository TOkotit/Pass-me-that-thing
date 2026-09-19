using Game.Scripts.GameFiles.Entity.Buildings;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings
{
    public class DoorBuilding : Building
    {
        [Inject]
        public void Construct(BuildingsDatabase buildingsDatabase)
        {
            BuildingData = buildingsDatabase.GetBuildingFromAll("door");
        }
        public override void OnDeath()
        {
            NetworkServer.Destroy(transform.parent.gameObject);
            Debug.Log($"[door] OnDeath");
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            Debug.Log($"[door] OnHealthChanged {currentHealth} / {maxHealth}");
        }
    }
}
