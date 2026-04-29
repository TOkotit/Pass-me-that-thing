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
                scope.Container.InjectGameObject(gameObject);
            }
            else
            {
                Debug.LogError("GameplayScope not found!");
            }
        }
    }
}