using System;
using Game.Scripts.GameFiles.InteractableObjects;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNodePort : WireNode
    {
        [SerializeField] private PortType portType;
        
        [SerializeField] private float availableValue;
        [SerializeField] private float requiredValue;

        private bool _isOn = true;

        public float AvailableValue => availableValue;
        public float RequiredValue => requiredValue;
        
        public bool IsOn
        {
            get => _isOn;
            set
            {
                _isOn = value;
                RecalculateNet();
            }
        }

        public event Action<bool> OnWireNetStateChanged;

        public void OnWireNetWorkingStateChanged(bool isNetWorking)
        {
            OnWireNetStateChanged?.Invoke(isNetWorking);
        }
        
        public void RecalculateNet()
        {
            if (isServer && NetId != -1)
                _wireManager.WireNets[NetId].Recalculate();
        }
        
    }
}