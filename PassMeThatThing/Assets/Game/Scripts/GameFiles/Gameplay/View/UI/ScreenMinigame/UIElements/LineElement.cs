using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    [UxmlElement("LineElement")]
    public partial class LineElement : VisualElement
    {
        private Vector2 _startPoint;
        private Vector2 _endPoint;
        
        

        public LineElement()
        {
            //generateVisualContent += OnGenerateVisualContent;
        }
        
        public LineElement(Vector2 start, Vector2 end, VectorImage image, float wireWidth)
        : this()
        {
            _startPoint = start;
            _endPoint = end;
            style.backgroundImage = new StyleBackground(image);
            style.height = wireWidth;

            style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(50));

            UpdatePositions(_startPoint, _endPoint);
        }

        public void UpdatePositions(Vector2 newStart, Vector2 newEnd)
        {
            if (_startPoint != newStart || _endPoint != newEnd)
            {
                _startPoint = newStart;
                _endPoint = newEnd;

                var direction = newEnd - newStart;
                var newLen = (newEnd - newStart).magnitude;

                var _currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                style.rotate = new StyleRotate(new Rotate(new Angle(_currentAngle, AngleUnit.Degree)));
                style.width = newLen;

                style.left = newStart.x;
                style.top = newStart.y;

                MarkDirtyRepaint();
            }
        }

        //private void OnGenerateVisualContent(MeshGenerationContext mgc)
        //{
        //    var painter = mgc.painter2D;

        //    painter.lineWidth = _lineWidth;
        //    painter.strokeColor = _color;
        //    painter.lineCap = LineCap.Round; 

        //    painter.BeginPath();
        //    painter.MoveTo(_startPoint);
        //    painter.LineTo(_endPoint);
        //    painter.Stroke();
        //}
    }
}