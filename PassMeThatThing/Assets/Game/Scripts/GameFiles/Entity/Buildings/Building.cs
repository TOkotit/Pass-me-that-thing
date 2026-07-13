using System;
using Entity;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    /// <summary>
    /// базовый класс для всего что может построить игрок и что могут сломать мобы
    /// </summary>
    public class Building : Damagable
    {
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
            
        }
        
        
    }
}