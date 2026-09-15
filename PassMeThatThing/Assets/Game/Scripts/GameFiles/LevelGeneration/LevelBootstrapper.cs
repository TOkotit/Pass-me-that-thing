using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelBootstrapper : NetworkBehaviour
    {
        [SerializeField] private LevelOrchestrator orchestrator;
        [SerializeField] private LevelConfig _config;

        [ContextMenu("Generate Level")]
        public void Generate()
        {
            
            if (!orchestrator)
            {
                Debug.LogError("[СБОЙ] Ссылка на LevelOrchestrator не назначена в инспекторе.");
                return;
            }
            orchestrator.GenerateLevelFromConfig("BootStrapper");
        }

        private void Start()
        {
            Generate();
        }
    }
}