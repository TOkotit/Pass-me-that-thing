using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelBootstrapper : MonoBehaviour
    {
        [SerializeField] private LevelPlacementOrchestrator placementOrchestrator;
        
        private LevelMacroData levelSettings = new();

        private void Start()
        {
            RunGeneration();
        }

        [ContextMenu("Run Generation")]
        public void RunGeneration()
        {
            if (placementOrchestrator == null)
            {
                Debug.LogError("Не назначена ссылка Placement Orchestrator в LevelBootstrapper");
                return;
            }

            if (levelSettings == null)
            {
                Debug.LogError("Не назначены настройки уровня (LevelMacroData) в LevelBootstrapper");
                return;
            }

            var graphBuilder = new LevelGraphBuilder();
            var rootHubNode = graphBuilder.BuildGraph(levelSettings);
        
            placementOrchestrator.GeneratePhysicalLevel(rootHubNode);
        }
    }
}