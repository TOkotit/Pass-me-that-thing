using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class LocalWireHandlerModel
    {
        //nodeId, entryId
        private Queue<(int, int)> _highlightedNodesId = new ();
        private List<WireType> _highlightedNodesTypes = new();


        public Queue<(int, int)> HighlightedNodesId => _highlightedNodesId;

        public List<WireType> HighlightedNodesTypes => _highlightedNodesTypes;


        //nodeId, entryId
        public event Action<int, int> OnWireNodeHighlighted;

        //firstNodeId, secondNodeId, firstEntryId, secondEntryId
        public event Action<int, int, int, int> OnWireNodePairMatched; 

        //nodeId
        public event Action<int> OnWireNodeCleared;

        public event Action<List<WireType>> OnTypesChanged;


        private void ClearNode(int nodeId)
        {
            Debug.Log($"[W] ClearNode {nodeId}");
            OnWireNodeCleared?.Invoke(nodeId);
        }

        private void Highlight(int nodeId, int entryId)
        {
            Debug.Log($"[W] Highlight {nodeId}");
            OnWireNodeHighlighted?.Invoke(nodeId, entryId);
        }

        private void MatchPair(int firstNodeId, int secondNodeId, int firstEntryId, int secondEntryId)
        {
            Debug.Log($"[W] MatchPair {firstNodeId} {secondNodeId}");
            OnWireNodePairMatched?.Invoke(firstNodeId, secondNodeId, firstEntryId, secondEntryId);
        }


        public void HighlightNode(int nodeId, int entryId, WireType type)
        {
            Debug.Log($"[W] highlighted node {nodeId} {entryId}");

            //нода уже выбрана
            if (_highlightedNodesId.Any(x => x.Item1 == nodeId))
            {
                _highlightedNodesId.Clear();
                ClearNode(nodeId);

                _highlightedNodesTypes.Clear();
                OnTypesChanged?.Invoke(_highlightedNodesTypes);
                
            }
            else //не выбрана
            {
                _highlightedNodesId.Enqueue((nodeId, entryId));
                Highlight(nodeId, entryId);

                _highlightedNodesTypes.Add(type);
                OnTypesChanged?.Invoke(_highlightedNodesTypes);

                if (_highlightedNodesId.Count == 2) // выбрано 2 ноды
                {
                    var first = _highlightedNodesId.Dequeue();
                    var second = _highlightedNodesId.Dequeue();

                    MatchPair(first.Item1, second.Item1, first.Item2, second.Item2);


                    _highlightedNodesTypes.Clear();
                    OnTypesChanged?.Invoke(_highlightedNodesTypes);
                }
            }
            
        }

        public void CancelHighlight()
        {
            _highlightedNodesId.Clear();

            _highlightedNodesTypes.Clear();
            OnTypesChanged?.Invoke(_highlightedNodesTypes);
        }

        
    }
}