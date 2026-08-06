using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelBootstrapperNew : MonoBehaviour
    {
        [SerializeField] private LevelOrchestrator orchestrator;
        [SerializeField] private LevelGraphConfig graphConfig = new LevelGraphConfig();

        [ContextMenu("Generate Level")]
        public void Generate()
        {
            if (orchestrator == null)
            {
                Debug.LogError("[СБОЙ] Ссылка на LevelOrchestrator не назначена в инспекторе.");
                return;
            }

            var generator = new LevelGenerator(graphConfig);
            var clusters = generator.GenerateClusters();

            if (clusters != null && clusters.Count > 0)
            {
                orchestrator.GeneratePhysicalLevel(clusters);
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