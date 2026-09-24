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

        public int LevelInDayAmount => levelInDayAmount;
        public List<LevelData> LevelsData => levelsData;

        public float RestDuration => restDuration;

        public float GetStageDuration(GlobalStagesType type, int level)
        {
            var duration = type switch
            {
                GlobalStagesType.Preparation => GetLevelData(level-1).PreparationPhaseTime,
                GlobalStagesType.Fight => GetLevelData(level-1).FightPhaseTime,
                GlobalStagesType.Rest => RestDuration,
                _ => 5f
            };
            return duration;
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

        public LevelData GetLevelData(int levelIndex)
        {
            Debug.Log($"dayIndex{levelIndex}");
            if (levelIndex < levelsData.Count)
                return levelsData[levelIndex];
            else
            {
                return levelsData.Last();
            }
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