using Game.Scripts.GameFiles.Entity.Buildings;
using System.Collections;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.EventBuildings
{
    public class BigPipes : Building
    {

        [Inject]
        public void Construct(BuildingsDatabase buildingsDatabase)
        {
            BuildingData = buildingsDatabase.GetBuildingFromAll("bigPipes");
        }
        public override void OnDeath()
        {
            Debug.Log($"[BigPipes] OnDeath");
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            Debug.Log($"[BigPipes] OnHealthChanged {currentHealth} / {maxHealth}");
        }



    }
}