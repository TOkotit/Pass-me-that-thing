using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public struct WireNetNetworkData
    {
        public float availableQuantity;
        public float requiredQuantity;
        public bool isWorking;


        public WireNetNetworkData(float availableQuantity, float requiredQuantity)
        {
            this.availableQuantity = availableQuantity;
            this.requiredQuantity = requiredQuantity;
            isWorking = availableQuantity >= requiredQuantity;
        }
    }
}