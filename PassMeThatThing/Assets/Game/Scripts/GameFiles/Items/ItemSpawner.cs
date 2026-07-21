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
        [SerializeField] private ItemPoolManager reserveItemPoolManager; //удалить потом
        public ItemData Item {get => item; set => item = value;}
        public Transform PointToSpawn {get => pointToSpawn; set => pointToSpawn = value;}
        
        
        [Inject] private ItemPoolManager _itemPoolManager; 
        [Inject] private PhysicalItemRegistry _physicalItemRegistry;
        
        
        [Command(requiresAuthority = false)] 
        private void CmdInteractWithObject()
        {
            var itemToDrop = _itemPoolManager.CreateNewObject(item.Id);
            itemToDrop.transform.position = pointToSpawn.position;

            var physItem = itemToDrop.GetComponent<PhysicalItem>();
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
        public void SpawnItem() //код для тестовой сцены, не работает. потом удалить
        {
            if (!_itemPoolManager || _physicalItemRegistry == null)
            {
                _itemPoolManager = reserveItemPoolManager;
                _physicalItemRegistry = PhysicalItemRegistry.Instance;
            }

            var itemToDrop = _itemPoolManager.CreateNewObject(item.Id);
            if (!itemToDrop) return;
            itemToDrop.transform.position = pointToSpawn.position;
            itemToDrop.SetActive(true);
            var physItem = itemToDrop.GetComponent<PhysicalItem>();
            if (physItem)
                _physicalItemRegistry.Register(physItem);
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