using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelBootstrapper : MonoBehaviour
    {
        [SerializeField] private LevelPlacementOrchestrator placementOrchestrator;
        public static LevelGraphBuilder GraphBuilder;
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
            GraphBuilder = new LevelGraphBuilder();
            var rootHubNode = GraphBuilder.BuildGraph(levelSettings);
        
            placementOrchestrator.GeneratePhysicalLevel(rootHubNode);
        }
    }
}