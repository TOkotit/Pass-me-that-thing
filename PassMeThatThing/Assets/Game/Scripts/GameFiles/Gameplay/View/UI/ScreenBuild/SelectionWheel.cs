using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scripts.GameFiles.Gameplay.View.UI.ScreenBuild
{
    [UxmlElement("SelectionWheel")]
    public partial class SelectionWheel : VisualElement
    {
        private readonly List<VisualElement> _segmentElements = new();
    
        private int _segmentsCount = 10;
        private float _radius = 100f;
        private int _hoveringElemIndex = -1;
        
        private List<Sprite> _segmentSprites = new();

        public event Action<int, int> OnValueChanged;
        public event Action<int, int> OnPreviewValueChanged;

        public const string SegmentClassName = "selection-wheel__segment";
        public const string SelectedSegmentClassName = "selection-wheel__segment--selected";
        

        public SelectionWheel()
        {
            pickingMode = PickingMode.Position;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public void OnGeometryChanged(GeometryChangedEvent e)
        {
            RefreshWheel();
        }
        
        public void SetImageSprites(List<Sprite> sprites)
        {
            _segmentSprites = sprites;
            
            RefreshWheel();
        }

        public void RefreshWheel()
        {
            //Debug.Log("RefreshWheel");
            Clear();
            _segmentElements.Clear();
            _hoveringElemIndex = -1;

            if (_segmentsCount <= 0) return;

            _radius = contentRect.width / 5;
            
            var stepAngle = 360f / _segmentsCount;

            for (int i = 0; i < _segmentsCount; i++)
            {
                var segment = new VisualElement();
                segment.AddToClassList(SegmentClassName);
                
                segment.style.position = Position.Absolute;
                
                var angleDeg = (i + 0.5f) * stepAngle;
                var angleRad = angleDeg * Mathf.Deg2Rad;

                var x = contentRect.width / 2 
                        + Mathf.Cos(angleRad) * _radius;
                var y = contentRect.height / 2 
                        - Mathf.Sin(angleRad) * _radius;

                segment.style.left = x;
                segment.style.top = y;
                
                Add(segment);
                _segmentElements.Add(segment);
            }

            if (_segmentSprites.Count > 0)
            {
                for (int i = 0; i < _segmentElements.Count && i < _segmentSprites.Count; i++)
                {
                    _segmentElements[i].style.backgroundImage = new StyleBackground(_segmentSprites[i]);
                }
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            int index = CalculateSegmentIndex(evt.localPosition);
            if (index >= 0)
            {
                OnValueChanged?.Invoke(index, _segmentsCount - 1);
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            int wheelPartIndex = CalculateSegmentIndex(evt.localPosition);

            if (wheelPartIndex != _hoveringElemIndex)
            {
                if (_hoveringElemIndex >= 0 && _hoveringElemIndex < _segmentElements.Count)
                {
                    _segmentElements[_hoveringElemIndex].RemoveFromClassList(SelectedSegmentClassName);
                }

                _hoveringElemIndex = wheelPartIndex;

                if (_hoveringElemIndex >= 0 && _hoveringElemIndex < _segmentElements.Count)
                {
                    _segmentElements[_hoveringElemIndex].AddToClassList(SelectedSegmentClassName);
                }

                OnPreviewValueChanged?.Invoke(wheelPartIndex, _segmentsCount - 1);
            }
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (_hoveringElemIndex >= 0 && _hoveringElemIndex < _segmentElements.Count)
            {
                _segmentElements[_hoveringElemIndex].RemoveFromClassList(SelectedSegmentClassName);
                _hoveringElemIndex = -1;
            }
        }
        
        private int CalculateSegmentIndex(Vector2 localMousePos)
        {
            if (_segmentsCount <= 0) return -1;
            
            var center = contentRect.size / 2f;
            var dir = localMousePos - center;
            
            var angleDelta = Vector2.SignedAngle(Vector2.right, new Vector2(dir.x, -dir.y));
            var positiveAngleDelta = (360f + angleDelta) % 360f;

            return (int)(positiveAngleDelta / (360f / _segmentsCount));
        }
    }
}