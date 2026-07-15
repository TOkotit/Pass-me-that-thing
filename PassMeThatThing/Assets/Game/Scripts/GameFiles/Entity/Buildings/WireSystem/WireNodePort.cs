using System;
using Game.Scripts.GameFiles.InteractableObjects;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNodePort : WireNode
    {
        [SerializeField] private PortType portType;
        
        [SerializeField] private float availableValue;
        [SerializeField] private float requiredValue;
        

        public float AvailableValue => availableValue;
        public float RequiredValue => requiredValue;
        
        public event Action<bool> OnWireNetStateChanged;

        public void OnWireNetWorkingStateChanged(bool isNetWorking)
        {
            OnWireNetStateChanged?.Invoke(isNetWorking);
        }
        
        public void RecalculateNet()
        {
            if (isServer)
                _wireManager.WireNets[NetId].Recalculate();
        }
        
    }
}