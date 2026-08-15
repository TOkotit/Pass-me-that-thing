using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class WireDragAndDropManipulator : PointerManipulator
    {
        private readonly string _slotContainerName;
        private readonly string _slotClassName;
        private Action<VisualElement, VisualElement> _onDrop;
        private LineElement _line;
        private VisualElement _box;
        private WireColorSpritesData _t;

        private bool _isDragging;

        private Vector2 _pointerStartPanel;
        private Vector2 _elementStartWorld;

        public WireDragAndDropManipulator(
            VisualElement target,
            Action<VisualElement, VisualElement> onDrop = null,
            string slotContainerName = "SlotsContainer",
            string slotClassName = "slot",
            LineElement line = null,
            VisualElement box = null,
            WireColorSpritesData t = null)
        {
            this.target = target;
            _onDrop = onDrop;
            _slotContainerName = slotContainerName;
            _slotClassName = slotClassName;
            _line = line;
            _box = box;
            _t = t;
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            
            target.style.position = Position.Absolute;
            
            _isDragging = true;

            _pointerStartPanel = evt.position;
            _elementStartWorld = target.worldBound.position;
            
            target.BringToFront();
            
            target.CapturePointer(evt.pointerId);
            
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging || !target.HasPointerCapture(evt.pointerId))
                return;
            var parent = target.parent;
            if (parent == null) 
                return;
            
            var pointerCurrent = (Vector2)evt.position;
            var pointerDelta = pointerCurrent - _elementStartWorld;
            
            var newWorld = _elementStartWorld + pointerDelta;
            
            var newLocal = parent.WorldToLocal(newWorld);
            
            target.style.left = newLocal.x;
            target.style.top = newLocal.y;
            
            if (_line != null && _box != null)
            {
                _line.UpdatePositions(new Vector2(_box.worldBound.position.x + _box.worldBound.width,
                    _box.worldBound.position.y), target.worldBound.position);
                _box.style.scale = new Vector2(1, 1);
                _box.style.backgroundImage = new StyleBackground(_t.start);
            }
                
            
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging || !target.HasPointerCapture(evt.pointerId))
                return;
            
            target.ReleasePointer(evt.pointerId);
            
            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (!_isDragging)
                return;
            
            var closestSlot = FindClosestSlot(requireOverlap: true);

            if (closestSlot == null)
            {
                SnapBackToStart();
                return;
            }
            
            SnapToSlotCenter(closestSlot);
            
            _isDragging = false;
            _onDrop?.Invoke(target, closestSlot);
        }

        private VisualElement FindClosestSlot(bool requireOverlap)
        {
            if (target.panel == null)
                return null;

            var root = target.panel.visualTree;
            var slotsRoot = string.IsNullOrEmpty(_slotContainerName)
                ? root
                : root.Q<VisualElement>(_slotContainerName);

            if (slotsRoot == null)
                return null;
            var slots = slotsRoot.Query<VisualElement>(className: _slotClassName).ToList();
            if (slots.Count == 0)
                return null;
            
            VisualElement closest = null;
            var bestDistance = float.MaxValue;

            foreach (var slot in slots)
            {
                if (requireOverlap && !target.worldBound.Overlaps(slot.worldBound))
                    continue;
                var distance =
                    (slot.worldBound.center - target.worldBound.center).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    closest = slot;
                }
            }
            
            return closest;
        }

        private void SnapToSlotCenter(VisualElement slot)
        {
            if (target.parent == null)
                return;
            
            var slotCenterWorld = slot.worldBound.center;
            var itemSize = new Vector2(target.resolvedStyle.width, target.resolvedStyle.height);
            
            var desiredWorld = slotCenterWorld - (itemSize * 0.5f);
            var desiredLocal = target.parent.WorldToLocal(desiredWorld);
            
            target.style.left = desiredLocal.x;
            target.style.top = desiredLocal.y;
            
            if (_line != null && _box != null)
            {
                _line.UpdatePositions(new Vector2(_box.worldBound.position.x + _box.worldBound.width,
                    _box.worldBound.position.y), target.worldBound.position);
                _box.style.scale = new Vector2(1, 1);
                _box.style.backgroundImage = new StyleBackground(_t.start);
            }
                
        }

        private void SnapBackToStart()
        {
            if (target.parent == null)
                return;
            
            //var localStart = target.parent.WorldToLocal(_elementStartWorld);
            
            target.style.left = 0f;
            target.style.top = 0f;
            
            if (_line != null && _box != null)
            {
                _line.UpdatePositions(_box.worldBound.position,
                    _box.worldBound.position);

                _box.transform.scale = new Vector2(-1, 1);
                _box.style.backgroundImage = new StyleBackground(_t.end);
            }
        }
    }
}