using System;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class LocalWireHandlerModel
    {
        private WireNode _highlightedNode;

        public event Action<WireNode> OnWireNodeHighlighted;

        public WireNode CurrentNode
        {
            get => _highlightedNode;
            set
            {
                if (_highlightedNode != value) OnWireNodeHighlighted?.Invoke(value);
                _highlightedNode = value;
            }
        }
    }
}