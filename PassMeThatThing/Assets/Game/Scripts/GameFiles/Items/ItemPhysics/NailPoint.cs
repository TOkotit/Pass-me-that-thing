using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    [RequireComponent(typeof(Collider))]
    public class NailPoint : NetworkBehaviour
    {
        [SerializeField] private Nail nail; 
        [SerializeField] private PointType pointType;

        private void OnTriggerEnter(Collider other)
        {
            
            if (pointType == PointType.Tip)
            {
                nail.OnTipHit(other);
            }

            if (pointType == PointType.Hat)
            {
                nail.OnHatHit(other);
            }
        }

        private enum PointType
        {
            Hat,
            Tip
        }
    }
}