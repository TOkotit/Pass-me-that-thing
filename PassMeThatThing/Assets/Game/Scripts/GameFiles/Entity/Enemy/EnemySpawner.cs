using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemySpawner : NetworkBehaviour
    {
        //[SerializeField] private List<Transform> zombieSpawnPositions;
        [SerializeField] private int enemyLimit;
        private int _enemyCount;
        
        //public List<Transform> ZombieSpawnPositions => zombieSpawnPositions;

        private List<EnemySpawnPoint> _enemySpawnPositions = new();

        public int EnemyCount
        {
            get => _enemyCount;
            set => _enemyCount = value;
        }

        [Server]
        public void RegisterSpawnpoint(EnemySpawnPoint sp)
        {
            _enemySpawnPositions.Add(sp);
        }

        [Server]
        public void UnRegisterSpawnpoint(EnemySpawnPoint sp)
        {
            _enemySpawnPositions.Remove(sp);
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
            var positions = _enemySpawnPositions;
            
            //if (enemyData.Id == "zombie")
            //{
            //    positions = zombieSpawnPositions;
            //}
            
            for (var i = 0; i < positions.Count && i < enemiesData.Count; i++)
            {
                SpawnEnemy(positions[i].transform.position, enemiesData[i]);

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