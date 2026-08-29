using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Game.Scripts.GameFiles.Items;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private ItemSpawner enemyDropItemSpawner;

        [SerializeField] private int enemyLimit;

        private int _enemyCount;

        private List<EnemySpawnPoint> _enemySpawnPositions = new();

        public int EnemyCount
        {
            get => _enemyCount;
            set => _enemyCount = value;
        }
        public ItemSpawner EnemyDropItemSpawner => enemyDropItemSpawner;

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
            var positions = _enemySpawnPositions;
            
            for (var i = 0; i < positions.Count && i < enemiesData.Count; i++)
            {
                SpawnEnemy(positions[i].transform.position, enemiesData[i]);
            }
        }

    }
}