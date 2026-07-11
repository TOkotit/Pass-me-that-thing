using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.LevelGeneration.Editor_Grid
{
    public class TestGrid : MonoBehaviour
    {
        [SerializeField] private LevelGrid levelGrid;

        public void Awake()
        {
            for (var i = 0; i < 20; i++)
            {
                levelGrid.SetCellState(new Vector3Int(Random.Range(0, 20), 0, Random.Range(0, 20)), true);
                
            }
        }
    }
}