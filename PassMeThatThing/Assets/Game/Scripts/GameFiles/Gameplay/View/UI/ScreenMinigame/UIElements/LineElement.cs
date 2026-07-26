using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    [UxmlElement("LineElement")]
    public partial class LineElement : VisualElement
    {
        private Vector2 _startPoint;
        private Vector2 _endPoint;
        private Color _color;
        private float _lineWidth;

        public LineElement()
        {
            generateVisualContent += OnGenerateVisualContent;
        }
        
        public LineElement(Vector2 start, Vector2 end, Color color, float width)
        : this()
        {
            _startPoint = start;
            _endPoint = end;
            _color = color;
            _lineWidth = width;
        }

        public void UpdatePositions(Vector2 newStart, Vector2 newEnd)
        {
            if (_startPoint != newStart || _endPoint != newEnd)
            {
                _startPoint = newStart;
                _endPoint = newEnd;

                MarkDirtyRepaint();
            }
        }
        
        public void UpdateEndPos(PointerMoveEvent e)
        {
            Debug.Log($"UpdateEndPos {_startPoint} {(Vector2)e.position}");
            
            if (_endPoint != (Vector2)e.position)
            {
                _endPoint = e.position;
                MarkDirtyRepaint();
            }
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;

            painter.lineWidth = _lineWidth;
            painter.strokeColor = _color;
            painter.lineCap = LineCap.Round; 

            painter.BeginPath();
            painter.MoveTo(_startPoint);
            painter.LineTo(_endPoint);
            painter.Stroke();
        }
    }
}