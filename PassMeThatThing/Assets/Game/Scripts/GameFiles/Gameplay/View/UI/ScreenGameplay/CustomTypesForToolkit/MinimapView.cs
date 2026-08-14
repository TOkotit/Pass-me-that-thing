using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scripts.GameFiles.LevelGeneration.UI
{
    public class MinimapView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<MinimapView, UxmlTraits> { }
 
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlFloatAttributeDescription _cellSize =
                new() { name = "cell-size", defaultValue = 12f };
 
            private readonly UxmlFloatAttributeDescription _viewportWidth =
                new() { name = "viewport-width", defaultValue = 240f };
 
            private readonly UxmlFloatAttributeDescription _viewportHeight =
                new() { name = "viewport-height", defaultValue = 240f };
 
            private readonly UxmlBoolAttributeDescription _showGridLines =
                new() { name = "show-grid-lines", defaultValue = true };
 
            private readonly UxmlColorAttributeDescription _cellColor =
                new() { name = "cell-color", defaultValue = new Color(0.30f, 0.85f, 0.40f) };
 
            private readonly UxmlColorAttributeDescription _gridLineColor =
                new() { name = "grid-line-color", defaultValue = new Color(1f, 1f, 1f, 0.08f) };
 
            private readonly UxmlColorAttributeDescription _backgroundColor =
                new() { name = "background-color", defaultValue = new Color(0.08f, 0.08f, 0.08f) };
            
            private readonly UxmlColorAttributeDescription _wallColor =
                new() { name = "wall-color", defaultValue = Color.white };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var minimap = (MinimapView)ve;
 
                minimap.CellColor = _cellColor.GetValueFromBag(bag, cc);
                minimap.GridLineColor = _gridLineColor.GetValueFromBag(bag, cc);
                minimap.BackgroundColor = _backgroundColor.GetValueFromBag(bag, cc);
                minimap.ShowGridLines = _showGridLines.GetValueFromBag(bag, cc);
                minimap.WallColor = _wallColor.GetValueFromBag(bag, cc);
                
                minimap.SetViewportInternal(
                    _viewportWidth.GetValueFromBag(bag, cc),
                    _viewportHeight.GetValueFromBag(bag, cc),
                    _cellSize.GetValueFromBag(bag, cc));
            }
        }
 
        public Color CellColor { get; set; } = new(0.30f, 0.85f, 0.40f);
        public Color GridLineColor { get; set; } = new(1f, 1f, 1f, 0.08f);
        public Color BackgroundColor { get; set; } = new(0.08f, 0.08f, 0.08f);
        public Color WallColor { get; set; } = Color.white;
 
        public bool ShowGridLines { get; set; } = true;
 
        private float _viewportWidth = 240f;
 
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
 
        public float CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = Mathf.Max(1f, value);
                MarkDirtyRepaint();
            }
        }
 
        public Vector3Int Center { get; private set; }
 
        private bool _centerIsManual;
        private Vector2 _panRemainder;
 
        private readonly Dictionary<Vector3Int, LevelGrid.CellData> _cellsDataCache = new();
        private LevelGrid _levelGrid;
        private readonly List<Vector3Int> _cellsCache = new();
        
        private VisualElement _mapContent;
 
        public MinimapView()
        {
            style.position = Position.Absolute;
            style.overflow = Overflow.Hidden;

            style.borderTopLeftRadius = new StyleLength(Length.Percent(50));
            style.borderTopRightRadius = new StyleLength(Length.Percent(50));
            style.borderBottomLeftRadius = new StyleLength(Length.Percent(50));
            style.borderBottomRightRadius = new StyleLength(Length.Percent(50));

            _mapContent = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50))
                }
            };
            Add(_mapContent);

            UpdateViewportSize();
            
            _mapContent.generateVisualContent += OnGenerateVisualContent;
        }
        
        public void SetRotation(float angleDegrees)
        {
            _mapContent.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(angleDegrees)));
        }
 
        private void SetViewportInternal(float viewportWidth, float viewportHeight, float cellSize)
        {
            _viewportWidth = Mathf.Max(1f, viewportWidth);
            _viewportHeight = Mathf.Max(1f, viewportHeight);
            _cellSize = Mathf.Max(1f, cellSize);
            UpdateViewportSize();
        }
 
        private void UpdateViewportSize()
        {
            style.width = _viewportWidth;
            style.height = _viewportHeight;
            MarkDirtyRepaint();
        }
 
        public void SetCenter(Vector3Int worldCell)
        {
            _centerIsManual = true;

            if (Center == worldCell) return;
            
            Center = worldCell;
            _panRemainder = Vector2.zero;
            MarkDirtyRepaint();
        }
 
        public void MoveCenter(Vector3Int cellDelta)
        {
            SetCenter(Center + cellDelta);
        }
 
        public void Pan(Vector2 screenPixelDelta)
        {
            _panRemainder += screenPixelDelta;
 
            var cellDeltaX = Mathf.RoundToInt(_panRemainder.x / _cellSize);
            var cellDeltaY = Mathf.RoundToInt(_panRemainder.y / _cellSize);
            if (cellDeltaX == 0 && cellDeltaY == 0) return;
 
            _panRemainder -= new Vector2(cellDeltaX * _cellSize, cellDeltaY * _cellSize);
 
            Center += new Vector3Int(-cellDeltaX, 0, cellDeltaY);
            _centerIsManual = true;
            MarkDirtyRepaint();
        }
 
        public void ResetAutoCenter()
        {
            _centerIsManual = false;
            _panRemainder = Vector2.zero;
            RecalculateAutoCenter();
            MarkDirtyRepaint();
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
 
            MarkDirtyRepaint();
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
 
        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            if (_viewportWidth <= 0f || _viewportHeight <= 0f) return;

            var painter = ctx.painter2D;
            
            var diagonal = Mathf.Sqrt(_viewportWidth * _viewportWidth + _viewportHeight * _viewportHeight);

            DrawFilledRect(painter, -diagonal, -diagonal, diagonal * 3f, diagonal * 3f, BackgroundColor);

            if (ShowGridLines)
            {
                DrawGridLines(painter, diagonal);
            }

            DrawOccupiedCells(painter, diagonal);
        }
 
        private void DrawGridLines(Painter2D painter, float diagonal)
        {
            var pivotX = _viewportWidth * 0.5f;
            var pivotY = _viewportHeight * 0.5f;
 
            painter.strokeColor = GridLineColor;
            painter.lineWidth = 1f;
 
            var startX = Mod(pivotX, _cellSize);
            for (var x = startX - diagonal; x <= _viewportWidth + diagonal; x += _cellSize)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, -diagonal));
                painter.LineTo(new Vector2(x, _viewportHeight + diagonal));
                painter.Stroke();
            }
 
            var startY = Mod(pivotY, _cellSize);
            for (var y = startY - diagonal; y <= _viewportHeight + diagonal; y += _cellSize)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(-diagonal, y));
                painter.LineTo(new Vector2(_viewportWidth + diagonal, y));
                painter.Stroke();
            }
        }
 
        private void DrawOccupiedCells(Painter2D painter, float diagonal)
        {
            if (_cellsCache.Count == 0) return;

            var pivotX = _viewportWidth * 0.5f;
            var pivotY = _viewportHeight * 0.5f;
            var maxDist = (diagonal * 0.5f) + _cellSize;

            var padding = Mathf.Max(1f, _cellSize * 0.1f);

            foreach (var cell in _cellsCache)
            {
                var localX = cell.x - Center.x;
                var localZ = Center.z - cell.z;

                var x = pivotX + localX * _cellSize;
                var y = pivotY + localZ * _cellSize;

                if (Mathf.Abs(x - pivotX) > maxDist || Mathf.Abs(y - pivotY) > maxDist) continue;

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
                var localX = cell.x - Center.x;
                var localZ = Center.z - cell.z;

                var x = pivotX + localX * _cellSize;
                var y = pivotY + localZ * _cellSize;

                if (Mathf.Abs(x - pivotX) > maxDist || Mathf.Abs(y - pivotY) > maxDist) continue;
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