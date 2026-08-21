using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelBootstrapperNew : MonoBehaviour
    {
        [SerializeField] private LevelOrchestrator orchestrator;
        [SerializeField] private LevelGraphConfig graphConfig = new LevelGraphConfig();
        
        [Header("Seed Settings")]
        [SerializeField] private int testSeed = 12345; 
        [SerializeField] private bool useRandomSeedForTests = false;

        [ContextMenu("Generate Level")]
        public void Generate()
        {
            if (orchestrator == null)
            {
                Debug.LogError("[СБОЙ] Ссылка на LevelOrchestrator не назначена в инспекторе.");
                return;
            }

            var currentSeed = useRandomSeedForTests ? Random.Range(int.MinValue, int.MaxValue) : testSeed;
            Debug.Log($"[BOOTSTRAPPER] Генерация уровня. Используемый Seed: {currentSeed}");

            var generator = new LevelGenerator(graphConfig, currentSeed);
            var clusters = generator.GenerateClusters();

            if (clusters != null && clusters.Count > 0)
            {
                orchestrator.GeneratePhysicalLevel(clusters, currentSeed);
            }
            else
            {
                Debug.LogError("[СБОЙ] Не удалось сгенерировать кластеры уровня.");
            }
        }

        private void Start()
        {
            Generate();
        }
    }
}