using System;
using System.Collections.Generic;
using Assets.Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireVisualizer : MonoBehaviour
    {
        [SerializeField] private WireLineView wireLineViewPrefab;
        [SerializeField] private SerializedDictionary<WireType, Color> wireColors;
        
        private Dictionary<(int, int), WireLineView> wireLineViewContainer = new ();


        
        public void DrawNodeLines(WireNode firstNode, WireNode secondNode,
            WireNodeEntry firstEntry, WireNodeEntry secondEntry)
        {

            if (firstNode.WireType == WireType.Electricity)
            {
                var temp = Instantiate(wireLineViewPrefab);

                temp.plug1.SetActive(true);
                temp.plug2.SetActive(true);

                temp.plug1.transform.position = firstEntry.EntryView.transform.position;
                temp.plug1.transform.rotation = firstEntry.EntryView.transform.rotation;

                temp.plug2.transform.position = secondEntry.EntryView.transform.position;
                temp.plug2.transform.rotation = secondEntry.EntryView.transform.rotation;

                temp.lineRenderer.SetPosition(0, temp.wirePoint1.position);
                temp.lineRenderer.SetPosition(1, temp.wirePoint2.position);

                temp.lineRenderer.startColor = wireColors[firstNode.WireType];
                temp.lineRenderer.endColor = wireColors[firstNode.WireType];

                wireLineViewContainer[(firstNode.NodeId, secondNode.NodeId)] = temp;
            }
            else
            {
                var temp = Instantiate(wireLineViewPrefab);

                temp.lineRenderer.SetPosition(0, firstNode.transform.position);
                temp.lineRenderer.SetPosition(1, secondNode.transform.position);

                temp.lineRenderer.startColor = wireColors[firstNode.WireType];
                temp.lineRenderer.endColor = wireColors[firstNode.WireType];

                wireLineViewContainer[(firstNode.NodeId, secondNode.NodeId)] = temp;
            }

            
        }

        public void ClearNodeLines(WireNode firstNode)
        {
            //Todo переделать на поиск соседних а не всех
            
            var toRemove = new List<(int, int)>();
            foreach (var line in wireLineViewContainer)
            {
                if (line.Key.Item1 == firstNode.NodeId || line.Key.Item2 == firstNode.NodeId)
                {
                    Destroy(line.Value.gameObject);
                    toRemove.Add(line.Key);
                }
            }

            foreach (var line in toRemove)
            {
                wireLineViewContainer.Remove(line);
            }
        }
    }
}