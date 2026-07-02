using DI;
using UnityEngine;
using VContainer.Unity;

namespace Game.Scripts.Systems
{
    public class AutoGameplayScopeInjector : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"AutoGameplayScopeInjector AWAKE {gameObject.name}");
            var scope = LifetimeScope.Find<GameplayScope>();
    
            if (scope != null)
            {
                
                if (scope.Container != null)
                {
                    Debug.Log($"AutoGameplayScopeInjector {gameObject.name} injected");
                    InjectAllComponents(scope);
                }
                else
                {
                    Debug.Log($"<color=red>SCOPE.CONTAINER is null");
                    StartCoroutine(WaitAndInject(scope));
                }
            }
            else
            {
                Debug.Log($"<color=red>SCOPE is null");
            }
        }

        private System.Collections.IEnumerator WaitAndInject(LifetimeScope scope)
        {
            while (scope.Container == null) yield return null;
            InjectAllComponents(scope);
        }

        private void InjectAllComponents(LifetimeScope scope)
        {
            // Находим абсолютно все компоненты (включая NetworkBehaviour), 
            // даже если они на дочерних объектах
            var components = GetComponentsInChildren<MonoBehaviour>(true);
            
            foreach (var component in components)
            {
                // Пропускаем сам инжектор
                if (component == this) continue; 
                
                scope.Container.Inject(component);
            }
            
            Debug.Log($"<color=orange>[DI] {gameObject.name} (and children) successfully injected.");
        }
    }
}