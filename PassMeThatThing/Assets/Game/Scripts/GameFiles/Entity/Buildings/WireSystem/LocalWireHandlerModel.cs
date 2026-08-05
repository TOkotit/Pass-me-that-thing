using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class LocalWireHandlerModel
    {
        //nodeId, entryId
        private Queue<(int, int)> _highlightedNodesId = new ();

        public Queue<(int, int)> HighlightedNodesId => _highlightedNodesId;

        //nodeId, entryId
        public event Action<int, int> OnWireNodeHighlighted;

        //firstNodeId, secondNodeId, firstEntryId, secondEntryId
        public event Action<int, int, int, int> OnWireNodePairMatched; 

        //nodeId
        public event Action<int> OnWireNodeCleared;

        public event Action<int> OnWireNodeCount;


        public void HighlightNode(int nodeId, int entryId)
        {
            Debug.Log($"[W] highlighted node {nodeId} {entryId}");

            //нода уже выбрана
            if (_highlightedNodesId.Any(x => x.Item1 == nodeId))
            {
                _highlightedNodesId.Clear();
                OnWireNodeCleared?.Invoke(nodeId);
                OnWireNodeCount?.Invoke(_highlightedNodesId.Count);
            }
            else //не выбрана
            {
                _highlightedNodesId.Enqueue((nodeId, entryId));
                OnWireNodeHighlighted?.Invoke(nodeId, entryId);
                OnWireNodeCount?.Invoke(_highlightedNodesId.Count);

                if (_highlightedNodesId.Count == 2) // выбрано 2 ноды
                {
                    var first = _highlightedNodesId.Dequeue();
                    var second = _highlightedNodesId.Dequeue();

                    OnWireNodePairMatched?.Invoke(first.Item1, second.Item1, first.Item2, second.Item2);
                    OnWireNodeCount?.Invoke(_highlightedNodesId.Count);

                    Debug.Log($"[W] OnWireNodePairMatched?.Invoke");
                }
            }
            
        }

        public void ClearNode(int nodeId)
        {
            Debug.Log($"[W] ClearNode {nodeId}");
            OnWireNodeCleared?.Invoke(nodeId);
        }

        public void CancelHighlight()
        {
            _highlightedNodesId.Clear();
            OnWireNodeCount?.Invoke(_highlightedNodesId.Count);
        }
    }
}