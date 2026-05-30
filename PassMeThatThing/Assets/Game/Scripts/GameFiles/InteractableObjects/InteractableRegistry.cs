using System.Collections.Generic;
using Game.Scripts.GameFiles.InteractableObjects;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public class InteractableRegistry
    {
        public static InteractableRegistry Instance { get; private set; }

        private readonly Dictionary<GameObject, Interactable> _interactables = new();

        public InteractableRegistry()
        {
            Instance = this;
        }

        public void Register(GameObject obj, Interactable interactable)
        {
            if (!obj || !interactable)
                return;

            if (!_interactables.ContainsKey(obj))
            {
                _interactables.Add(obj, interactable);
                Debug.Log($"[Int-s] {obj.name} registered as interactable.");
            }
        }

        public void Unregister(GameObject obj)
        {
            if (!obj)
                return;

            if (_interactables.ContainsKey(obj))
                _interactables.Remove(obj);
        }

        public Interactable GetInteractable(GameObject obj)
        {
            _interactables.TryGetValue(obj, out var interactable);
            return interactable;
        }

        public bool TryGetInteractable(GameObject obj, out Interactable interactable)
        {
            return _interactables.TryGetValue(obj, out interactable);
        }
    }
}