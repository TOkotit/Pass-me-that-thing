using System;
using Assets.Game.Scripts.GameFiles.Entity.Buildings;
using Entity;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    /// <summary>
    /// базовый класс для всего что может построить игрок и что могут сломать мобы
    /// </summary>
    public class Building : Damageable
    {
        [SerializeField] private BuildingView buildingView;

        protected BuildingModel BuildingModel;
        protected BuildingData BuildingData;
        
        public override DamagableModel DamagableModel => BuildingModel;

        public virtual void Awake()
        {
            BuildingModel = new BuildingModel();
        }

        public new void Start()
        {
            base.Start();
            
            if (isServer)
            {
                ServerSetMaxHealth(BuildingData.maxHealth, true); //SO
            }
        }

        public override void OnDeath()
        {
            
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            buildingView.TakeDamage();
        }
        
        
    }
}