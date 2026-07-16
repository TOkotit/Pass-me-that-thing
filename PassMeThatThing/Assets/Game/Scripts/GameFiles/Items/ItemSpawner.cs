using DG.Tweening;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemSpawner : NetworkBehaviour ,Interactable
    {
        [SerializeField] private Transform pointToSpawn;
        [SerializeField] private ItemData item;
        public ItemData Item {get => item; set => item = value;}
        
        
        private ItemPoolManager _itemPoolManager; 
        private PhysicalItemRegistry _physicalItemRegistry;
        [Inject]
        private void Construct(NetworkManager networkManager, PhysicalItemRegistry itemRegistry)
        {
            _itemPoolManager = networkManager.GetComponent<ItemPoolManager>();
            _physicalItemRegistry = itemRegistry;
        }
        
        [Command(requiresAuthority = false)] 
        private void CmdInteractWithObject()
        {
            var itemToDrop = _itemPoolManager.GetFromPool(item.Id);
            itemToDrop.transform.position = pointToSpawn.position;
            itemToDrop.SetActive(true);
            _physicalItemRegistry.Register(itemToDrop.GetComponent<PhysicalItem>());
            NetworkServer.Spawn(itemToDrop);

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
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}