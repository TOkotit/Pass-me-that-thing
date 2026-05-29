using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.Highlight
{
    public class OutlineRegistry
    {
        public static OutlineRegistry Instance { get; private set; }
        private Dictionary<GameObject, Outline> _outlines = new Dictionary<GameObject, Outline>();
        private List<Outline> _enabledOutlines = new List<Outline>();
        
        public OutlineRegistry()
        {
            Instance = this;
        }

        public List<Outline> EnabledOutlines => _enabledOutlines;

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

        public Outline TryGetOutline(GameObject outline, out Outline outlineComponent)
        {
            if (_outlines.ContainsKey(outline))
            {
                outlineComponent = _outlines[outline];
                return _outlines[outline];
            }
            outlineComponent = null;
            return null;
        }

        public void EnableOutline(Outline outline)
        {
            outline.enabled = true;
            EnabledOutlines.Add(outline);
        }
        
        public void DisableOutline(Outline outline)
        {
            outline.enabled = false;
            EnabledOutlines.Remove(outline);
        }
    }
}