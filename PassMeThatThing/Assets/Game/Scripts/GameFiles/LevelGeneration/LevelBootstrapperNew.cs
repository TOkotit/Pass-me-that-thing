using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelBootstrapperNew : MonoBehaviour
    {
        [SerializeField] private LevelPlacementOrchestratorNew orchestrator;
        [SerializeField] private LevelGraphConfig graphConfig = new LevelGraphConfig();

        [ContextMenu("Generate Level")]
        public void Generate()
        {
            var builder = new LevelGraphBuilderNew(graphConfig);
            var graphResult = builder.GenerateGraph();

            if (graphResult.IsValid && graphResult.Root != null)
            {
                orchestrator.GeneratePhysicalLevel(graphResult.Root);
            }
            else
            {
                Debug.LogError("[СБОЙ] Не удалось сгенерировать граф уровня.");
            }
        }

        private void Start()
        {
            Generate();
        }
    }
}