using DI;
using Entity;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class VaultDoorDamagable : Damagable
    {
        [Inject] private VaultDoorDamagableModel _model;
        [Inject] private DamagableRegistry _damagableRegistry;
        public override DamagableModel DamagableModel { get => _model;}
        
        
        // private void Awake()
        // {
        //     var scope = LifetimeScope.Find<GameplayScope>();
        //     if (scope)
        //         scope.Container.Inject(this);
        // }
        
        
        public override void OnDeath()
        {
            Destroy(gameObject);
        }

        public override void OnHealthChanged(int currentHealth,  int maxHealth)
        {
            Debug.Log($"Дверь повреждена! {currentHealth}/{maxHealth}");
        }
    }
}