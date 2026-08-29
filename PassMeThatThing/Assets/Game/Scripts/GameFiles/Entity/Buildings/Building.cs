using System;
using Assets.Game.Scripts.GameFiles.Entity.Buildings;
using Entity;
using Mirror;
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
            else if (isClient)
            {
                ClientInitMaxHealth(BuildingData.maxHealth, true);
            }
        }

        public override void OnDeath()
        {
            if (isServer)
            {
                NetworkServer.Destroy(gameObject);
            }
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            
        }

        public override void OnTakeDamage(int deltaHp)
        {
            base.OnTakeDamage(deltaHp);
            buildingView.TakeDamage();
        }

        public override void OnHeal(int deltaHp)
        {
            base.OnHeal(deltaHp);
            buildingView.Repair();
        }

    }
}