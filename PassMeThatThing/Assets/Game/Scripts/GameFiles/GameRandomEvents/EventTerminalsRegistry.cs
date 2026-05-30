using System.Collections.Generic;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events
{
    public class EventTerminalsRegistry
    {
        public static EventTerminalsRegistry Instance { get; private set; }
        private Dictionary<GameObject, EventTerminal> _eventTerminals = new ();

        public EventTerminalsRegistry()
        {
            Instance = this;
        }
        public void Register(EventTerminal terminal)
        {
            var terminalGameObject = terminal.gameObject;
            if (!_eventTerminals.ContainsKey(terminalGameObject))
                _eventTerminals.Add(terminalGameObject, terminal); 
            Debug.Log($"<color=blue> [EvTermsReg]{terminal.gameObject.name} has been registered");
        }
        
        public void Unregister(EventTerminal terminal)
        {
            var terminalGameObject = terminal.gameObject;
            if (_eventTerminals.ContainsKey(terminalGameObject))
                _eventTerminals.Remove(terminalGameObject);
        }

        public EventTerminal GetItem(GameObject terminal)
        {
            if (_eventTerminals.ContainsKey(terminal))
            {
                return _eventTerminals[terminal];
            }
            return null;
        }
        public bool TryGetItem(GameObject terminalObject, out EventTerminal terminal)
        {
            if (_eventTerminals.ContainsKey(terminalObject))
            {
                terminal = _eventTerminals[terminalObject];
                return terminal;
            }
            terminal = null;
            return terminal;
        }
    }
}