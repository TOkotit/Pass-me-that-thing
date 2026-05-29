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
                    scope.Container.InjectGameObject(gameObject);
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
            scope.Container.InjectGameObject(gameObject);
            Debug.Log($"[DI] {gameObject.name} successfully injected after waiting.");
        }
    }
}