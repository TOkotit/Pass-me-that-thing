using DG.Tweening;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemSpawner : NetworkBehaviour, Interactable
    {
        [SerializeField] private Transform pointToSpawn;
        [SerializeField] private ItemData item;
        public ItemData Item {get => item; set => item = value;}
        
        
        [Inject] private ItemPoolManager _itemPoolManager; 
        [Inject] private PhysicalItemRegistry _physicalItemRegistry;
        
        
        [Command(requiresAuthority = false)] 
        private void CmdInteractWithObject()
        {
            var itemToDrop = _itemPoolManager.CreateNewObject(item.Id);
            itemToDrop.transform.position = pointToSpawn.position;

            var physItem = itemToDrop.GetComponent<PhysicalItem>();
            // physItem.Network.ItemData = item;
            _physicalItemRegistry.Register(physItem);

            //RpcInteractWithObject();
        }

        [ClientRpc]
        public void RpcInteractWithObject()
        {
            gameObject.transform.DOScale(0f, 0.5f).SetEase(Ease.InBounce)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
        }
        
        public void Interact()
        {
            CmdInteractWithObject();
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