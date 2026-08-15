using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scripts.GameFiles.LevelGeneration.UI
{
    [UxmlElement("MinimapView")]
    public partial class MinimapView : VisualElement
    {
        
        [UxmlAttribute("cell-color")]
        public Color CellColor { get; set; } = new(0.30f, 0.85f, 0.40f);
        
        [UxmlAttribute("grid-line-color")]
        public Color GridLineColor { get; set; } = new(1f, 1f, 1f, 0.08f);
        
        [UxmlAttribute("background-color")]
        public Color BackgroundColor { get; set; } = new(0.08f, 0.08f, 0.08f);
        
        [UxmlAttribute("wall-color")]
        public Color WallColor { get; set; } = Color.white;
        
        [UxmlAttribute("show-grid-lines")]
        public bool ShowGridLines { get; set; } = true;
 
        private float _viewportWidth = 240f;
 
        [UxmlAttribute("viewport-width")]
        public float ViewportWidth
        {
            get => _viewportWidth;
            set
            {
                _viewportWidth = Mathf.Max(1f, value);
                UpdateViewportSize();
            }
        }
 
        private float _viewportHeight = 240f;
 
        [UxmlAttribute("viewport-height")]
        public float ViewportHeight
        {
            get => _viewportHeight;
            set
            {
                _viewportHeight = Mathf.Max(1f, value);
                UpdateViewportSize();
            }
        }
 
        private float _cellSize = 12f;
 
        [UxmlAttribute("cell-size")]
        public float CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = Mathf.Max(1f, value);
                MarkDirtyRepaint();
                _mapContent.MarkDirtyRepaint(); 
                _gridLayer.MarkDirtyRepaint();
            }
        }
 
        public Vector3Int Center { get; private set; }
 
        private bool _centerIsManual;
        private Vector2 _panRemainder;
 
        private readonly Dictionary<Vector3Int, LevelGrid.CellData> _cellsDataCache = new();
        private LevelGrid _levelGrid;
        private readonly List<Vector3Int> _cellsCache = new();
        
        private readonly VisualElement _filterContainer;
        private readonly VisualElement _mapContent;
        private VisualElement _gridLayer;
 
        public MinimapView()
        {
            style.position = Position.Absolute;
            style.overflow = Overflow.Hidden;
            style.translate = new StyleTranslate(new Translate(Length.Percent(-50), Length.Percent(-50)));
            
            
            style.borderTopLeftRadius = new StyleLength(Length.Percent(50));
            style.borderTopRightRadius = new StyleLength(Length.Percent(50));
            style.borderBottomLeftRadius = new StyleLength(Length.Percent(50));
            style.borderBottomRightRadius = new StyleLength(Length.Percent(50));
            
            _filterContainer = new VisualElement
            {
                name = "minimap-filter-container",
                style =
                {
                    flexGrow = 1,
                    width = Length.Percent(100),
                    height = Length.Percent(100),
                    overflow = Overflow.Hidden, 
                    backgroundColor = new Color(0, 0, 0, 0)
                }
            };
            _filterContainer.AddToClassList("minimap-filter-class");
            Add(_filterContainer);
            
            _mapContent = new VisualElement
            {
                name = "minimap-content",
                style =
                {
                    flexGrow = 1,
                    transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50))
                }
            };
            _filterContainer.Add(_mapContent);
            
            _gridLayer = new VisualElement
            {
                name = "minimap-grid",
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    top = 0,
                    right = 0,
                    bottom = 0
                }
            };
            _mapContent.Add(_gridLayer);
            
            UpdateViewportSize();

            _mapContent.generateVisualContent += OnGenerateBackgroundContent;
            _gridLayer.generateVisualContent += OnGenerateGridContent;
        }
        
        public void SetRotation(float angleDegrees)
        {
            _mapContent.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(angleDegrees)));
        }
 
        private void UpdateViewportSize()
        {
            style.width = _viewportWidth;
            style.height = _viewportHeight;
            MarkDirtyRepaint();
        }
        
        private void UpdateGridTranslate()
        {
            var pivotX = _viewportWidth * 0.5f;
            var pivotY = _viewportHeight * 0.5f;
            var halfCell = _cellSize * 0.5f;

            var offsetX = pivotX - Center.x * _cellSize - halfCell;
            var offsetY = pivotY + Center.z * _cellSize - halfCell;

            _gridLayer.style.translate = new StyleTranslate(new Translate(offsetX, offsetY));
        }
 
        public void SetCenter(Vector3Int worldCell)
        {
            _centerIsManual = true;

            if (Center == worldCell) return;
            
            Center = worldCell;
            _panRemainder = Vector2.zero;
            UpdateGridTranslate();
        }
 
        public void SetSource(LevelGrid levelGrid)
        {
            _levelGrid = levelGrid;
            Refresh();
        }
 
        public void Refresh()
        {
            _cellsCache.Clear();
            _cellsDataCache.Clear();
 
            if (_levelGrid != null)
            {
                _cellsCache.AddRange(_levelGrid.OccupiedCells);
                foreach (var cell in _cellsCache)
                {
                    if (_levelGrid.TryGetCellData(cell, out var data))
                    {
                        _cellsDataCache[cell] = data;
                    }
                }
            }
 
            if (!_centerIsManual)
            {
                RecalculateAutoCenter();
            }
 
            UpdateGridTranslate();
            _gridLayer.MarkDirtyRepaint();
        }
 
        private void RecalculateAutoCenter()
        {
            if (_cellsCache.Count == 0)
            {
                Center = Vector3Int.zero;
                return;
            }
 
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minZ = int.MaxValue;
            var maxZ = int.MinValue;
 
            foreach (var cell in _cellsCache)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.x > maxX) maxX = cell.x;
                if (cell.z < minZ) minZ = cell.z;
                if (cell.z > maxZ) maxZ = cell.z;
            }
 
            Center = new Vector3Int((minX + maxX) / 2, 0, (minZ + maxZ) / 2);
        }
        
        private void OnGenerateBackgroundContent(MeshGenerationContext ctx)
        {
            if (_viewportWidth <= 0f || _viewportHeight <= 0f) return;

            var painter = ctx.painter2D;
            var diagonal = Mathf.Sqrt(_viewportWidth * _viewportWidth + _viewportHeight * _viewportHeight);

            DrawFilledRect(painter, -diagonal, -diagonal, diagonal * 3f, diagonal * 3f, BackgroundColor);

            if (ShowGridLines)
            {
                DrawGridLines(painter, diagonal);
            }
        }
 
        private void DrawGridLines(Painter2D painter, float diagonal)
        {
            var pivotX = _viewportWidth * 0.5f;
            var pivotY = _viewportHeight * 0.5f;
            var halfCell = _cellSize * 0.5f;

            painter.strokeColor = GridLineColor;
            painter.lineWidth = 1f;
 
            var startX = Mod(pivotX - halfCell, _cellSize);
            for (var x = startX - diagonal; x <= _viewportWidth + diagonal; x += _cellSize)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, -diagonal));
                painter.LineTo(new Vector2(x, _viewportHeight + diagonal));
                painter.Stroke();
            }
 
            var startY = Mod(pivotY - halfCell, _cellSize);
            for (var y = startY - diagonal; y <= _viewportHeight + diagonal; y += _cellSize)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(-diagonal, y));
                painter.LineTo(new Vector2(_viewportWidth + diagonal, y));
                painter.Stroke();
            }
        }
        
        private void OnGenerateGridContent(MeshGenerationContext ctx)
        {
            if (_cellsCache.Count == 0) return;

            var painter = ctx.painter2D;
            var padding = Mathf.Max(1f, _cellSize * 0.1f);

            foreach (var cell in _cellsCache)
            {
                var x = cell.x * _cellSize;
                var y = -cell.z * _cellSize;

                DrawFilledRect(painter, x + padding, y + padding, _cellSize - padding * 2, _cellSize - padding * 2, CellColor);
            }

            painter.strokeColor = WallColor;
            painter.lineWidth = Mathf.Max(4f, _cellSize * 0.25f);

            var directions = new (Vector3Int dir, Vector2 p1, Vector2 p2)[]
            {
                (new Vector3Int(0, 0, 1), new Vector2(0, 0), new Vector2(1, 0)),
                (new Vector3Int(1, 0, 0), new Vector2(1, 0), new Vector2(1, 1)),
                (new Vector3Int(0, 0, -1), new Vector2(0, 1), new Vector2(1, 1)),
                (new Vector3Int(-1, 0, 0), new Vector2(0, 0), new Vector2(0, 1))
            };

            foreach (var cell in _cellsCache)
            {
                var x = cell.x * _cellSize;
                var y = -cell.z * _cellSize;

                if (!_cellsDataCache.TryGetValue(cell, out var cellData)) continue;

                foreach (var (dir, p1, p2) in directions)
                {
                    var neighborCell = cell + dir;
                    var hasNeighbor = _cellsDataCache.TryGetValue(neighborCell, out var neighborData);

                    var isSameRoom = hasNeighbor && neighborData.RoomId == cellData.RoomId;
                    var hasDoor = cellData.Doors != null && cellData.Doors.Contains(dir);

                    if (!isSameRoom || hasDoor)
                    {
                        var start = new Vector2(x + p1.x * _cellSize, y + p1.y * _cellSize);
                        var end = new Vector2(x + p2.x * _cellSize, y + p2.y * _cellSize);

                        if (hasDoor)
                        {
                            var segmentDir = end - start;
                            painter.BeginPath();
                            painter.MoveTo(start);
                            painter.LineTo(start + segmentDir * 0.3f);
                            painter.Stroke();

                            painter.BeginPath();
                            painter.MoveTo(start + segmentDir * 0.7f);
                            painter.LineTo(end);
                            painter.Stroke();
                        }
                        else if (!hasNeighbor || !isSameRoom)
                        {
                            painter.BeginPath();
                            painter.MoveTo(start);
                            painter.LineTo(end);
                            painter.Stroke();
                        }
                    }
                }
            }
        }
 
        private static float Mod(float a, float b)
        {
            return a - b * Mathf.Floor(a / b);
        }
 
        private static void DrawFilledRect(Painter2D painter, float x, float y, float width, float height, Color color)
        {
            if (width <= 0f || height <= 0f) return;
 
            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, y));
            painter.LineTo(new Vector2(x + width, y));
            painter.LineTo(new Vector2(x + width, y + height));
            painter.LineTo(new Vector2(x, y + height));
            painter.ClosePath();
            painter.Fill();
        }
    }
}