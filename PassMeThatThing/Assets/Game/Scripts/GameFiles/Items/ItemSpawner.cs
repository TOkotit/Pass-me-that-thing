using DG.Tweening;
using FishNet.Managing;
using FishNet.Object;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.ItemPhysics;

using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemSpawner : Interactable
    {
        [SerializeField] private Transform pointToSpawn;
        [SerializeField] private ItemData item;
        
        
        
        private ItemPoolManager _itemPoolManager; 
        private PhysicalItemRegistry _physicalItemRegistry;
        [Inject]
        private void Construct(NetworkManager networkManager, PhysicalItemRegistry itemRegistry)
        {
            _itemPoolManager = networkManager.GetComponent<ItemPoolManager>();
            _physicalItemRegistry = itemRegistry;
        }
        
        [ServerRpc(RequireOwnership = false)] 
        private void CmdInteractWithObject()
        {
            var itemToDrop = _itemPoolManager.GetFromPool(item.Id, pointToSpawn.position, Quaternion.identity);
            //itemToDrop.transform.position = pointToSpawn.position;
            //itemToDrop.SetActive(true);
            
            _physicalItemRegistry.Register(itemToDrop.GetComponent<PhysicalItem>());
            ServerManager.Spawn(itemToDrop);

            RpcInteractWithObject();
        }

        [ObserversRpc]
        public void RpcInteractWithObject()
        {
            gameObject.transform.DOScale(0f, 0.5f).SetEase(Ease.InBounce)
                .OnComplete(() =>
                {
                    
                    gameObject.SetActive(false);
                });
        }
        
        public override void Interact()
        {
            CmdInteractWithObject();
        }
        public override void SrbToggle()
        {
            
        }
    }
}