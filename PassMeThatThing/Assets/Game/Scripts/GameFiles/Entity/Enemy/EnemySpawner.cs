using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private List<Vector3> zombieSpawnPositions;


        private int _enemyCount;
        
        public List<Vector3> ZombieSpawnPositions => zombieSpawnPositions;

        public int EnemyCount => _enemyCount;

        [Server]
        public void SpawnEnemy(Vector3 pos, EnemyData enemyData)
        {
            var enemyInstance = Instantiate(enemyData.WorldPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
            var enemy = enemyInstance.GetComponent<Enemy>();
            // enemy _____ += OnEnemyDied;
            _enemyCount++;
        }

        [Server]
        public void SpawnWave(int count, EnemyData enemyData)
        {
            var positions = zombieSpawnPositions;
            
            if (enemyData.Id == "Zombie")
            {
                positions = zombieSpawnPositions;
            }
            
            for (var i = 0; i < positions.Count && i < count; i++)
            {
                SpawnEnemy(positions[i], enemyData);
            }
        }

        private void OnEnemyDied()
        {
            _enemyCount--;
        }
        
    }
}