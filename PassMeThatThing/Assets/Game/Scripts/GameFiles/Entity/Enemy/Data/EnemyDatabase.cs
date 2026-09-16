using Assets.Game.Scripts.GameFiles.GlobalStageManager;
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

    public EnemyData GetEnemy(string id)
    {
        return allEnemies.Find(enemy => enemy.Id == id);
    }

    public List<EnemyData> GetEnemiesByDiff(EnemyDifficulty diff)
    {
        return allEnemies.Where(enemy => enemy.EnemyDifficulty == diff).ToList();
    }
}

[Serializable]
public class LevelRange
{
    public int minVal;
    public int maxVal;
}
