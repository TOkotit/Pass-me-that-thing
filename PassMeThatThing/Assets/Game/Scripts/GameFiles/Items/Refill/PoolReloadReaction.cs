using System;
using System.Collections;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.Refill
{
    public class PoolReloadReaction : ItemReaction, IRefiller
    {
        [SerializeField] private RefillType refillType;
        [SerializeField] private int refillAmount;
        [SerializeField] private bool dropOnEmpty;
        
        public override void Act()
        {
            //типа
        }
        public RefillType RefillType { get => refillType; }
        public int RefillAmount { get => refillAmount; }
        public bool DropOnEmpty { get => dropOnEmpty; }
        public IEnumerator Refill(IRefillable target, PlayerInventory inventory)
        {
            yield return new WaitForSeconds(target.ReloadTime);
            var needed = target.MaxAmmo - target.CurrentAmmo;
            var ammoToReload = Math.Min(needed, refillAmount);
            target.CurrentAmmo += ammoToReload;
            refillAmount -= ammoToReload;
            if (dropOnEmpty && refillAmount <= 0)
            {
                inventory.ServerDeleteItem(_item.Network.instanceId);
            }
        }
    }
}