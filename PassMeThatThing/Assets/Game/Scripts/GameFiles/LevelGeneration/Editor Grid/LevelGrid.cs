using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.Editor_Grid
{
    
    [RequireComponent(typeof(Grid))]
    public class LevelGrid : MonoBehaviour
    {
        public struct CellData
        {
            public int RoomId;
            public List<Vector3Int> Doors;
            public RoomType RoomType;

        }

        private Dictionary<Vector3Int, CellData> _cellDataMap;
        public int editorDrawRadius = 20;
        [SerializeField] private Grid _grid;
        [SerializeField] private Color emptyCellColor = Color.gray;
        [SerializeField] private Color occupiedCellColor = Color.green;

        [SerializeField] private LevelOrchestrator levelOrchestrator;
        
        public static LevelGrid Instance { get; private set; }
        public LevelOrchestrator Orchestrator => levelOrchestrator;
        
        [SerializeField, HideInInspector] 
        private List<Vector3Int> _serializedOccupiedCells = new();
        
        public Grid UnityGrid => _grid;
        
        private HashSet<Vector3Int> _occupiedCells;
        
        public IReadOnlyCollection<Vector3Int> OccupiedCells
        {
            get
            {
                if (_occupiedCells == null) InitializeGrid();
                return _occupiedCells;
            }
        }


        
        private void Awake()
        {
            Instance = this;

            if (levelOrchestrator == null)
                levelOrchestrator = FindObjectOfType<LevelOrchestrator>();

            InitializeGrid();
        }
        
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        
        public IReadOnlyList<PlacedRoomDataCluster> AllPlacedRooms =>
            levelOrchestrator != null ? levelOrchestrator.AllPlacedRooms : Array.Empty<PlacedRoomDataCluster>();

        public IReadOnlyList<RoomCluster> Clusters =>
            levelOrchestrator != null ? levelOrchestrator.Clusters : Array.Empty<RoomCluster>();
        public int TotalRoomCount => AllPlacedRooms.Count;

        public int ClusterCount => Clusters.Count;

        public Dictionary<RoomType, int> GetRoomTypeCounts()
        {
            var counts = new Dictionary<RoomType, int>();

            foreach (var roomData in AllPlacedRooms)
            {
                if (roomData.RoomComponent == null) continue;

                var type = roomData.RoomComponent.RoomType;
                counts.TryGetValue(type, out var current);
                counts[type] = current + 1;
            }

            return counts;
        }

        public IEnumerable<PlacedRoomDataCluster> GetRoomsByType(RoomType type)
        {
            return AllPlacedRooms.Where(r => r.RoomComponent != null && r.RoomComponent.RoomType == type);
        }

        public IEnumerable<PlacedRoomDataCluster> GetRoomsInCluster(RoomCluster cluster)
        {
            return AllPlacedRooms.Where(r => r.Cluster == cluster);
        }


        public void InitializeGrid()
        {
            if (!_grid)
                _grid = GetComponent<Grid>();

            _occupiedCells ??= new HashSet<Vector3Int>(_serializedOccupiedCells);
            _cellDataMap ??= new Dictionary<Vector3Int, CellData>();
            
        }

        
        public void SetCellState(Vector3Int cellPosition, bool isOccupied, List<Vector3Int> doorDirections = null, int roomId = -1, RoomType roomType = RoomType.None)
        {
            
            
            if (_occupiedCells == null) InitializeGrid();

            if (isOccupied)
            {
                _occupiedCells?.Add(cellPosition);
                if (_cellDataMap != null)
                {
                    _cellDataMap[cellPosition] = new CellData 
                    { 
                        RoomId = roomId, 
                        Doors = doorDirections ?? new List<Vector3Int>(),
                        RoomType = roomType
                    };
                }
            }
            else
            {
                _occupiedCells?.Remove(cellPosition);
                _cellDataMap?.Remove(cellPosition);
            }
        }


        
        public bool TryGetCellData(Vector3Int cellPosition, out CellData data)
        {
            if (_cellDataMap != null)
            {
                return _cellDataMap.TryGetValue(cellPosition, out data);
            }
            data = default;
            return false;
        }

        
        
        public bool IsCellOccupied(Vector3Int cellPosition)
        {
            return _occupiedCells != null && _occupiedCells.Contains(cellPosition);
        }

        public void ClearGrid()
        {
            _occupiedCells?.Clear();
            _cellDataMap?.Clear();
            _serializedOccupiedCells.Clear();
        }
        
        public void BakeSerializedData()
        {
            if (_occupiedCells != null)
            {
                _serializedOccupiedCells = _occupiedCells.ToList();
            }
            
            #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
            #endif
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
                    
                    Gizmos.DrawWireCube(worldPos, cubeSize);
                }
            }
        }

    }
}