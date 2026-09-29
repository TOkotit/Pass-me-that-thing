using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using Game.Scripts.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Assets.Game.Scripts.GameFiles.GlobalStageManager
{
    [CreateAssetMenu(fileName = "GlobalStageDatabase", menuName = "Scriptable Objects/GlobalStageDatabase")]
    public class GlobalStageDatabase : ScriptableObject
    {
        [SerializeField] private int levelInDayAmount = 3;

        [SerializeField] private float restDuration;

        [SerializeField] private int baseQuotaConst = 50;
        [SerializeField] private int quotaSpreadPercent = 5;

        [Header("Конфиг уровней на диапазон левелов (включ./включ.)")]
        public SerializedDictionary<LevelRange, LevelData> levelConfig;



        public int LevelInDayAmount => levelInDayAmount;
        public float RestDuration => restDuration;
        public IReadOnlyDictionary<LevelRange, LevelData> LevelConfig => levelConfig;

        private int _maxConfigLevel = -1;

        public int MaxConfigLevel 
        {  
            get 
            {
                if (_maxConfigLevel == -1)
                {
                    foreach (var ld in levelConfig)
                    {
                        if (ld.Key.maxVal > _maxConfigLevel)
                        {
                            _maxConfigLevel = ld.Key.maxVal;
                        }
                    }
                }
                return _maxConfigLevel;
            }
        }

        public LevelData GetLevelData(int levelNumber)
        {
            Debug.Log($"level number {levelNumber}");

            if (levelNumber <= MaxConfigLevel)
            {
                var c = levelConfig
                    .FirstOrDefault(kp => kp.Key.minVal <= levelNumber
                        && levelNumber <= kp.Key.maxVal);

                return c.Value;
            }
            else
            {
                return levelConfig.Last().Value;
            }
        }


        public List<EnemyPackData> GetEnemyPacksByLevel(int level)
        {
            var c = GetLevelData(level);

            var enemyDiffPoints = c.EnemyDiffPoints;

            return GetRandomEnemypackByDiffPoints(enemyDiffPoints);
        }

        private List<EnemyPackData> GetRandomEnemypackByDiffPoints(int enemyDiffPoints)
        {
            var allDifficulties = Enum.GetValues(typeof(EnemyDifficulty))
                .Cast<EnemyDifficulty>()
                .ToList();

            var currPoints = enemyDiffPoints;
            var tempResult = new Dictionary<EnemyDifficulty, int>();

            while (currPoints > 0)
            {
                int randDiffIndex;

                if (currPoints == 1)
                {
                    randDiffIndex = 0;
                }
                else
                {
                    randDiffIndex = Random.Range(0, Math.Min(allDifficulties.Count, currPoints - 1));
                }
                
                if (tempResult.ContainsKey(allDifficulties[randDiffIndex]))
                {
                    tempResult[allDifficulties[randDiffIndex]]++;
                }
                else
                {
                    tempResult.Add(allDifficulties[randDiffIndex], 1);
                }

                currPoints = currPoints - 1 - randDiffIndex;
            }

            var result = new List<EnemyPackData>();

            foreach (var p in tempResult)
            {
                result.Add(new EnemyPackData() { enemyDiff=p.Key, count=p.Value });
            }

            return result;
        }

        public float GetStageDuration(GlobalStagesType type, int level)
        {
            var duration = type switch
            {
                GlobalStagesType.Preparation => GetLevelData(level).PreparationPhaseTime,
                GlobalStagesType.Fight => GetLevelData(level).FightPhaseTime,
                GlobalStagesType.Rest => RestDuration,
                _ => 5f
            };
            return duration;
        }

        public int GetQuotaSumByLevel(int level)
        {
            var c = GetLevelData(level);

            var quotaSum = c.QuotaPoints * baseQuotaConst;

            var randQuotaSum = RandomUtilities.RandNearMult(quotaSum, quotaSpreadPercent / 100f);

            return (int)randQuotaSum;
        }
    }

    [Serializable]
    public class LevelData
    {
        public float PreparationPhaseTime = 200f;
        public float FightPhaseTime = 300f;

        public int EnemyDiffPoints = 1;
        public int QuotaPoints = 1;

        [Header("wip")]
        public int TasksPoints = 1;
    }

    [Serializable]
    public struct EnemyPackData
    {
        public int count;
        public EnemyDifficulty enemyDiff;
    }
}