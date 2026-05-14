using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.Highlight
{
    public class OutlineRegistry
    {
        public static OutlineRegistry Instance { get; private set; }
        private Dictionary<GameObject, Outline> _outlines = new Dictionary<GameObject, Outline>();
        public OutlineRegistry()
        {
            Instance = this;
        }
        
        public void Register(Outline outline)
        {
            var outlineObject = outline.gameObject;
            if (!_outlines.ContainsKey(outlineObject))
                _outlines.Add(outlineObject, outline); 
        }
        
        
        public void Unregister(Outline outline)
        {
            var outlineObject = outline.gameObject;
            if (_outlines.ContainsKey(outlineObject))
                _outlines.Remove(outlineObject);
        }

        public Outline TryGetOutline(GameObject outline)
        {
            if (_outlines.ContainsKey(outline))
            {
                return _outlines[outline];
            }
            return null;
        }
    }
}