using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private Enemy defaultEnemyPrefab;

        public void SpawnEnemy(Enemy enemy)
        {
            
        }
    }
}