using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Mirror.SimpleWeb.Log;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Scriptable Objects/EnemyDatabase")]
public class EnemyDatabase : ScriptableObject
{
    public List<EnemyData> allEnemies;
    [Header("Конфиг мобов на диапазон левелов (включ./включ.)")]
    public SerializedDictionary<LevelRange, List<EnemyPackData>> levelEnemiesConfig;

    public EnemyData GetEnemy(string id)
    {
        return allEnemies.Find(enemy => enemy.Id == id);
    }

    public List<EnemyData> GetEnemiesByDiff(EnemyDifficulty diff)
    {
        return allEnemies.Where(enemy => enemy.EnemyDifficulty == diff).ToList();
    }

    public List<EnemyPackData> GetEnemyPacksByLevel(int level)
    {
        var c = levelEnemiesConfig
            .Where(kp => kp.Key.minVal <= level && level <= kp.Key.maxVal);
        if (c.Count() > 0)
        {
            return c.First().Value;
        }
        else
        {
            return levelEnemiesConfig.Last().Value;
        }
    }
}

[Serializable]
public class EnemyPackData
{
    public int count;
    public EnemyDifficulty enemyDiff;
}

[Serializable]
public class LevelRange
{
    public int minVal;
    public int maxVal;
}
