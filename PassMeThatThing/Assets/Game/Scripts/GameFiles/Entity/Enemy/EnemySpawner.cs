using System;
using System.Collections.Generic;
using System.Linq;
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

        public int EnemyCount
        {
            get => _enemyCount;
            set => _enemyCount = value;
        }


        [Server]
        public void SpawnEnemy(Vector3 pos, EnemyData enemyData)
        {
            var enemyInstance = Instantiate(enemyData.WorldPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
            var enemy = enemyInstance.GetComponent<Enemy>();
            
            enemy.EnemySpawner = this;
            
            _enemyCount++;
            Debug.Log($"Spawned enemy {enemyData.Id} + {_enemyCount}");
        }

        [Server]
        public void SpawnWave(List<EnemyData> enemiesData)
        {
            //Debug.Log($"Spawning wave of: {enemyData.Id}");
            var positions = zombieSpawnPositions;
            
            //if (enemyData.Id == "zombie")
            //{
            //    positions = zombieSpawnPositions;
            //}
            
            for (var i = 0; i < positions.Count && i < enemiesData.Count; i++)
            {
                SpawnEnemy(positions[i].position, enemiesData[i]);

                //if (_enemyCount < enemyLimit)
                //{
                //    SpawnEnemy(positions[i].position, enemiesData[i]);
                //}
                //else
                //{
                //    Debug.Log($"Enemy limit {_enemyCount}/{enemyLimit}");
                //}
            }
        }

        // private void Update()
        // {
        //     Debug.Log(_enemyCount);
        // }
    }
}