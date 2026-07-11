using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.Editor_Grid
{
    
    [RequireComponent(typeof(Grid))]
    public class LevelGrid : MonoBehaviour
    {
        [Header("Визуализация сетки")]
        public int editorDrawRadius = 20;
        [SerializeField] private Grid _grid;
        [SerializeField] private Color emptyCellColor = Color.gray;
        [SerializeField] private Color occupiedCellColor = Color.green;
        
        public Grid UnityGrid => _grid;
        
        private HashSet<Vector3Int> _occupiedCells;
        
        private void Awake()
        {
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            if (!_grid)
                _grid = GetComponent<Grid>();

            _occupiedCells ??= new HashSet<Vector3Int>();
        }
        
        public void SetCellState(Vector3Int cellPosition, bool isOccupied)
        {
            if (_occupiedCells == null) InitializeGrid();

            if (isOccupied)
            {
                _occupiedCells?.Add(cellPosition);
            }
            else
            {
                _occupiedCells?.Remove(cellPosition);
            }
        }
        
        public bool IsCellOccupied(Vector3Int cellPosition)
        {
            return _occupiedCells != null && _occupiedCells.Contains(cellPosition);
        }
        
        public void ClearGrid()
        {
            _occupiedCells?.Clear();
        }

        private void OnDrawGizmos()
        {
            if (UnityGrid == null) return;

            var cellSize = UnityGrid.cellSize;

            Gizmos.color = emptyCellColor;
            
            for (var i = -editorDrawRadius; i <= editorDrawRadius; i++)
            {
                var startZ = UnityGrid.CellToWorld(new Vector3Int(i, 0, -editorDrawRadius));
                var endZ = UnityGrid.CellToWorld(new Vector3Int(i, 0, editorDrawRadius));
                Gizmos.DrawLine(startZ, endZ);

                var startX = UnityGrid.CellToWorld(new Vector3Int(-editorDrawRadius, 0, i));
                var endX = UnityGrid.CellToWorld(new Vector3Int(editorDrawRadius, 0, i));
                Gizmos.DrawLine(startX, endX);
            }

            if (_occupiedCells is { Count: > 0 })
            {
                Gizmos.color = occupiedCellColor;
                var cubeSize = new Vector3(cellSize.x * 0.9f, 0.1f, cellSize.z * 0.9f);

                foreach (var cellPos in _occupiedCells)
                {
                    var worldPos = UnityGrid.GetCellCenterWorld(cellPos);
                    
                    worldPos.y -= cellSize.y * 0.5f; 
                    
                    Gizmos.DrawCube(worldPos, cubeSize);
                }
            }
        }
    }
}