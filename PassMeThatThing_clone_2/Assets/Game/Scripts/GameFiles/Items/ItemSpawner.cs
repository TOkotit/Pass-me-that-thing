using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemSpawner : NetworkBehaviour, IInteractable
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
        
        [Command(requiresAuthority = false)] 
        private void CmdInteractWithObject()
        {
            var itemToDrop = _itemPoolManager.GetFromPool(item.ID);
            itemToDrop.transform.position = pointToSpawn.position;
            itemToDrop.SetActive(true);
            _physicalItemRegistry.Register(itemToDrop.GetComponent<PhysicalItem>());
            NetworkServer.Spawn(itemToDrop);
        }
        
        public void Interact()
        {
            CmdInteractWithObject();
        }

        public void SrbToggle()
        {
            
        }
    }
}