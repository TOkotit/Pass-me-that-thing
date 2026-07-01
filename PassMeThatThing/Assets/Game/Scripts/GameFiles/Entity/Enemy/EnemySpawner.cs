using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private List<Transform> zombieSpawnPositions;
        [SerializeField] private int enemyLimit;
        private int _enemyCount;
        
        public List<Transform> ZombieSpawnPositions => zombieSpawnPositions;

        public int EnemyCount => _enemyCount;
        

        [Server]
        public void SpawnEnemy(Vector3 pos, EnemyData enemyData)
        {
            var enemyInstance = Instantiate(enemyData.WorldPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
            var enemy = enemyInstance.GetComponent<Enemy>();
            enemy.DamagableModel.OnDeath += OnEnemyDied;
            _enemyCount++;
            Debug.Log($"Spawned enemy {enemyData.Id}");
        }

        [Server]
        public void SpawnWave(int count, EnemyData enemyData)
        {
            Debug.Log($"Spawning wave of: {enemyData.Id}");
            var positions = zombieSpawnPositions;
            
            if (enemyData.Id == "zombie")
            {
                positions = zombieSpawnPositions;
            }
            
            for (var i = 0; i < positions.Count && i < count; i++)
            {
                if (_enemyCount < enemyLimit)
                {
                    SpawnEnemy(positions[i].position, enemyData);
                }
                else
                {
                    Debug.Log($"Enemy limit {_enemyCount}/{enemyLimit}");
                }
            }
        }

        private void OnEnemyDied()
        {
            _enemyCount--;
        }
        
    }
}