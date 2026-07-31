using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class LocalWireHandlerModel
    {
        private Queue<int> _highlightedNodesId = new ();

        public event Action<int> OnWireNodeHighlighted;
        
        public event Action<int, int> OnWireNodePairMatched;
        public event Action<int> OnWireNodeCleared;

        public event Action<int> OnWireNodeCount;

        public Queue<int> HighlightedNodesId => _highlightedNodesId;


        public void HighlightNode(int nodeId, WireNode node)
        {
            Debug.Log($"[W] highlighted node {nodeId}");
            if (_highlightedNodesId.Contains(nodeId))
            {
                _highlightedNodesId.Clear();
                OnWireNodeCleared?.Invoke(nodeId);
                OnWireNodeCount?.Invoke(_highlightedNodesId.Count);
            }
            else
            {
                _highlightedNodesId.Enqueue(nodeId);
                OnWireNodeHighlighted?.Invoke(nodeId);
                OnWireNodeCount?.Invoke(_highlightedNodesId.Count);
                if (_highlightedNodesId.Count == 2)
                {
                    OnWireNodePairMatched?.Invoke(_highlightedNodesId.Dequeue(), _highlightedNodesId.Dequeue());
                    OnWireNodeCount?.Invoke(_highlightedNodesId.Count);
                    Debug.Log($"[W] OnWireNodePairMatched?.Invoke");
                }
            }
            
        }

        public void ClearNode(int nodeId)
        {
            OnWireNodeCleared?.Invoke(nodeId);
        }

        public void CancelHighlight()
        {
            _highlightedNodesId.Clear();
            OnWireNodeCount?.Invoke(_highlightedNodesId.Count);
        }
    }
}