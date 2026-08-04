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
    /// Один узел системы коммуникаций(электричество, вода)
    /// </summary>
    public class WireNode : NetworkBehaviour
    {
        [SerializeField] private WireType wireType;

        [SerializeField] private int connLimit = 2;

        [SerializeField] private List<WireNodeEntry> _entries;
        
        [Inject] protected WireManager _wireManager;
        [Inject] private LocalWireHandlerModel _handlerModel;
        
        [SyncVar]
        private int _nodeId = -1;
        [SyncVar]
        private int _netId = -1;
        

        public int NodeId { get => _nodeId; set => _nodeId = value; }

        public int NetId { get => _netId; set => _netId = value; }

        public WireType WireType => wireType;

        public int ConnLimit => connLimit;

        public List<WireNodeEntry> Entries => _entries;

        public virtual void Start()
        {
            if (isServer)
            {
                _wireManager.RegisterNode(this);
            }

            foreach (var entry in Entries)
            {
                entry.OnEntryInteract += EntryInteracted;   
            }
        }

        public virtual void OnDestroy()
        {
            if (isServer)
            {
                _wireManager.UnRegisterNode(NodeId);
            }

            foreach (var entry in Entries)
            {
                entry.OnEntryInteract -= EntryInteracted;
            }
        }

        public void EntryInteracted(int entryId)
        {
            _handlerModel.HighlightNode(NodeId, entryId);
        }
    }
}