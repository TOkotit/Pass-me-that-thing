using Game.Scripts.GameFiles.InteractableObjects;
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

        [Inject]
        private void Construct(NetworkManager networkManager)
        {
            _itemPoolManager = networkManager.GetComponent<ItemPoolManager>();
        }
        
        [Command(requiresAuthority = false)] 
        private void CmdInteractWithObject()
        {
            var itemToDrop = _itemPoolManager.GetFromPool(item.ID);
        
            itemToDrop.transform.position = pointToSpawn.position;
            itemToDrop.SetActive(true);
        
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