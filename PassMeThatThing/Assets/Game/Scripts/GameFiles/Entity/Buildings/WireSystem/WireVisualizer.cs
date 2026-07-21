using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireVisualizer : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRendererPrefab;
        [SerializeField] private SerializedDictionary<WireType, Color> wireColors;
        
        private Dictionary<(int, int), LineRenderer> lineRenderersContainer = new ();


        
        public void DrawNodeLines(WireNode firstNode, WireNode secondNode)
        {
            
            var temp = Instantiate(lineRendererPrefab);

            temp.SetPosition(0, firstNode.transform.position);
            temp.SetPosition(1, secondNode.transform.position);
            
            temp.startColor = wireColors[firstNode.WireType];
            temp.endColor = wireColors[firstNode.WireType];
            
            lineRenderersContainer[(firstNode.NodeId, secondNode.NodeId)] = temp;
        }

        public void ClearNodeLines(WireNode firstNode)
        {
            //Todo переделать на поиск соседних а не всех
            
            var toRemove = new List<(int, int)>();
            foreach (var line in lineRenderersContainer)
            {
                if (line.Key.Item1 == firstNode.NodeId || line.Key.Item2 == firstNode.NodeId)
                {
                    Destroy(line.Value.gameObject);
                    toRemove.Add(line.Key);
                }
            }

            foreach (var line in toRemove)
            {
                lineRenderersContainer.Remove(line);
            }
        }
    }
}