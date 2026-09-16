using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Game.Scripts.GameFiles.GlobalStageManager
{
    [CreateAssetMenu(fileName = "GlobalStageDatabase", menuName = "Scriptable Objects/GlobalStageDatabase")]
    public class GlobalStageDatabase : ScriptableObject
    {
        [SerializeField] private int levelInDayAmount = 3;

        [SerializeField] private float restDuration;
        [SerializeField] private List<LevelData> levelsData;

        [Header("Конфиг мобов на диапазон левелов (включ./включ.)")]
        public SerializedDictionary<LevelRange, List<EnemyPackData>> levelEnemiesConfig;

        public int LevelAmount => levelInDayAmount;
        public List<LevelData> LevelsData => levelsData;

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

        public LevelData GetLevelData(int dayIndex)
        {
            return levelsData[dayIndex];
        }
    }

    [Serializable]
    public class LevelData
    {
        public float PreparationPhaseTime = 200f;
        public float FightPhaseTime = 300f;
    }

    [Serializable]
    public class EnemyPackData
    {
        public int count;
        public EnemyDifficulty enemyDiff;
    }
}