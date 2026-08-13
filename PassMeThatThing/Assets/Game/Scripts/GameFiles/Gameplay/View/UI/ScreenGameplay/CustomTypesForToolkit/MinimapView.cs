using System.Collections.Generic;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scripts.GameFiles.LevelGeneration.UI
{
    /// <summary>
    /// Миникарта уровня для UI Toolkit.
    ///
    /// Сам элемент - это и есть сетка: его пиксельный размер считается как
    /// GridCellsWidth/Height * CellSize, а не наоборот (не подгоняем клетки
    /// под заранее заданный размер контейнера).
    ///
    /// Разметка (линии сетки) и закрашенные клетки - два независимых слоя:
    /// линии можно выключить (ShowGridLines), на закрашенные клетки это не влияет,
    /// они всегда берутся из LevelGrid.OccupiedCells.
    /// </summary>
    public class MinimapView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<MinimapView, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlFloatAttributeDescription _cellSize =
                new() { name = "cell-size", defaultValue = 12f };

            private readonly UxmlIntAttributeDescription _gridCellsWidth =
                new() { name = "grid-cells-width", defaultValue = 20 };

            private readonly UxmlIntAttributeDescription _gridCellsHeight =
                new() { name = "grid-cells-height", defaultValue = 20 };

            private readonly UxmlBoolAttributeDescription _showGridLines =
                new() { name = "show-grid-lines", defaultValue = true };

            private readonly UxmlColorAttributeDescription _cellColor =
                new() { name = "cell-color", defaultValue = new Color(0.30f, 0.85f, 0.40f) };

            private readonly UxmlColorAttributeDescription _gridLineColor =
                new() { name = "grid-line-color", defaultValue = new Color(1f, 1f, 1f, 0.08f) };

            private readonly UxmlColorAttributeDescription _backgroundColor =
                new() { name = "background-color", defaultValue = new Color(0.08f, 0.08f, 0.08f) };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var minimap = (MinimapView)ve;

                minimap.CellColor = _cellColor.GetValueFromBag(bag, cc);
                minimap.GridLineColor = _gridLineColor.GetValueFromBag(bag, cc);
                minimap.BackgroundColor = _backgroundColor.GetValueFromBag(bag, cc);
                minimap.ShowGridLines = _showGridLines.GetValueFromBag(bag, cc);

                // Выставляем зум и размер сетки одним вызовом, чтобы не пересчитывать
                // style.width/height у элемента трижды подряд.
                minimap.SetGridSizeInternal(
                    _cellSize.GetValueFromBag(bag, cc),
                    _gridCellsWidth.GetValueFromBag(bag, cc),
                    _gridCellsHeight.GetValueFromBag(bag, cc));
            }
        }

        // ===== Внешний вид =====

        public Color CellColor { get; set; } = new(0.30f, 0.85f, 0.40f);
        public Color GridLineColor { get; set; } = new(1f, 1f, 1f, 0.08f);
        public Color BackgroundColor { get; set; } = new(0.08f, 0.08f, 0.08f);

        /// <summary>Рисовать ли линии разметки. Закрашенные клетки от этого не зависят -
        /// это просто визуальный фон, а не источник данных.</summary>
        public bool ShowGridLines { get; set; } = true;

        // ===== Зум и размер видимой сетки =====

        private float _cellSize = 12f;

        /// <summary>Видимый размер одной клетки на экране, в пикселях.
        /// Это и есть приближение/отдаление: больше значение - крупнее клетки,
        /// меньше - мельче. Меняй в рантайме, чтобы реализовать зум колесом мыши и т.п.</summary>
        public float CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = Mathf.Max(1f, value);
                UpdateElementSize();
            }
        }

        private int _gridCellsWidth = 20;

        /// <summary>Сколько клеток видно по ширине. Определяет размер самого элемента
        /// (и, соответственно, сколько всего влезет в кадр), а не размер уровня -
        /// её можно сделать заведомо больше застройки или меньше, для обрезки по краям.</summary>
        public int GridCellsWidth
        {
            get => _gridCellsWidth;
            set
            {
                _gridCellsWidth = Mathf.Max(1, value);
                UpdateElementSize();
            }
        }

        private int _gridCellsHeight = 20;

        /// <summary>Сколько клеток видно по высоте.</summary>
        public int GridCellsHeight
        {
            get => _gridCellsHeight;
            set
            {
                _gridCellsHeight = Mathf.Max(1, value);
                UpdateElementSize();
            }
        }

        /// <summary>Клетка мира (координата LevelGrid), которая сейчас находится
        /// в центре миникарты. По умолчанию пересчитывается автоматически как центр
        /// застройки при каждом Refresh(); вызови SetCenter(), чтобы взять управление
        /// на себя (например, для слежения за игроком).</summary>
        public Vector3Int Center { get; private set; }

        private bool _centerIsManual;

        // ===== Данные =====

        private LevelGrid _levelGrid;
        private readonly List<Vector3Int> _cellsCache = new();

        public MinimapView()
        {
            style.overflow = Overflow.Visible; // не обрезаем клетки, которые вышли за пределы сетки

            UpdateElementSize();
            generateVisualContent += OnGenerateVisualContent;
        }

        private void SetGridSizeInternal(float cellSize, int width, int height)
        {
            _cellSize = Mathf.Max(1f, cellSize);
            _gridCellsWidth = Mathf.Max(1, width);
            _gridCellsHeight = Mathf.Max(1, height);
            UpdateElementSize();
        }

        private void UpdateElementSize()
        {
            style.width = _gridCellsWidth * _cellSize;
            style.height = _gridCellsHeight * _cellSize;
            MarkDirtyRepaint();
        }

        /// <summary>Фиксирует клетку мира в центре миникарты и отключает автоцентровку.</summary>
        public void SetCenter(Vector3Int worldCell)
        {
            Center = worldCell;
            _centerIsManual = true;
            MarkDirtyRepaint();
        }

        /// <summary>Возвращает поведение по умолчанию - центр по границам застройки.</summary>
        public void ResetAutoCenter()
        {
            _centerIsManual = false;
            RecalculateAutoCenter();
            MarkDirtyRepaint();
        }

        /// <summary>Привязывает миникарту к конкретному LevelGrid и сразу отрисовывает его.</summary>
        public void SetSource(LevelGrid levelGrid)
        {
            _levelGrid = levelGrid;
            Refresh();
        }

        /// <summary>Перечитывает занятые клетки из LevelGrid и просит перерисовать элемент.
        /// Вызывай после того, как уровень сгенерирован (или перегенерирован).</summary>
        public void Refresh()
        {
            _cellsCache.Clear();

            if (_levelGrid != null)
            {
                _cellsCache.AddRange(_levelGrid.OccupiedCells);
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
            var width = _gridCellsWidth * _cellSize;
            var height = _gridCellsHeight * _cellSize;
            if (width <= 0f || height <= 0f) return;

            var painter = ctx.painter2D;

            DrawFilledRect(painter, 0f, 0f, width, height, BackgroundColor);

            if (ShowGridLines)
            {
                DrawGridLines(painter, width, height);
            }

            DrawOccupiedCells(painter, width, height);
        }

        private void DrawGridLines(Painter2D painter, float width, float height)
        {
            painter.strokeColor = GridLineColor;
            painter.lineWidth = 1f;

            for (var i = 0; i <= _gridCellsWidth; i++)
            {
                var x = i * _cellSize;
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, 0f));
                painter.LineTo(new Vector2(x, height));
                painter.Stroke();
            }

            for (var i = 0; i <= _gridCellsHeight; i++)
            {
                var y = i * _cellSize;
                painter.BeginPath();
                painter.MoveTo(new Vector2(0f, y));
                painter.LineTo(new Vector2(width, y));
                painter.Stroke();
            }
        }

        private void DrawOccupiedCells(Painter2D painter, float width, float height)
        {
            if (_cellsCache.Count == 0) return;

            // Пиксель, в который проецируется клетка Center - середина элемента.
            var pivotX = width * 0.5f;
            var pivotY = height * 0.5f;

            const float gap = 1f; // небольшой зазор между клетками, чтобы читалась разметка под ними

            foreach (var cell in _cellsCache)
            {
                var localX = cell.x - Center.x;
                // Мир: Z растёт "вверх", UI Toolkit: Y растёт вниз - инвертируем.
                var localZ = Center.z - cell.z;

                var x = pivotX + localX * _cellSize;
                var y = pivotY + localZ * _cellSize;

                DrawFilledRect(painter, x + gap, y + gap, _cellSize - gap * 2f, _cellSize - gap * 2f, CellColor);
            }
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