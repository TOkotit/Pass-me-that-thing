using DI;
using UnityEngine;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Items
{
    public class AutoGameplayScopeInjector : MonoBehaviour
    {
        private void Awake()
        {
            var scope = LifetimeScope.Find<GameplayScope>();
    
            if (scope != null)
            {
                if (scope.Container != null)
                {
                    InjectAllComponents(scope);
                }
                else
                {
                    StartCoroutine(WaitAndInject(scope));
                }
            }
        }

        private System.Collections.IEnumerator WaitAndInject(LifetimeScope scope)
        {
            while (scope.Container == null) yield return null;
            InjectAllComponents(scope);
        }

        private void InjectAllComponents(LifetimeScope scope)
        {
            var components = GetComponentsInChildren<MonoBehaviour>(true);
            
            foreach (var component in components)
            {
                if (component == this) continue; 
                
                scope.Container.Inject(component);
            }
            
            Debug.Log($"<color=orange>[DI] {gameObject.name} (and children) successfully injected.");
        }
    }
}