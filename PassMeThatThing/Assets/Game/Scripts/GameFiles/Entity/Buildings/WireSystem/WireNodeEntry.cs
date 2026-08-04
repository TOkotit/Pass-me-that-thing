using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    /// <summary>
    /// Вход в узел системы коммуникаций
    /// Если электричество то это сама розетка
    /// Если вода то сторона уголка трубы и тд
    /// </summary>
    public class WireNodeEntry : NetworkBehaviour, Interactable
    {
        [SerializeField] private GameObject entryView;

        [Inject] private WireManager wireManager;

        [SyncVar] 
        private int _entryId;

        public int EntryId { get => _entryId; set => _entryId = value; }
        public GameObject EntryView => entryView;

        public event Action<int> OnEntryInteract;

        private void Start()
        {
            if (isServer)
            {
                wireManager.RegisterEntry(this);
            }
        }

        private void OnDestroy()
        {
            if (isServer)
            {
                wireManager.UnregisterEntry(_entryId);
            }
        }

        public void Interact()
        {
            OnEntryInteract?.Invoke(EntryId);
        }

        public void SrbToggle()
        {
            
        }

        public void InteractWithItem(PhysicalItem item)
        {
            
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}