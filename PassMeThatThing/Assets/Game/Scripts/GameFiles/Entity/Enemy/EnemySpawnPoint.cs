using Game.Scripts.GameFiles.Entity.Enemy;
using System.Collections;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        [Inject] private EnemySpawner spawner;

        private void Start()
        {
            spawner.RegisterSpawnpoint(this);
        }

        private void OnDestroy()
        {
            spawner.UnRegisterSpawnpoint(this);
        }
    }
}