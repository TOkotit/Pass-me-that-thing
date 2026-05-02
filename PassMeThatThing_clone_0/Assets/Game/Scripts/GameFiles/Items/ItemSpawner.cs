using Game.Scripts.GameFiles.InteractableObjects;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items
{
    public class ItemSpawner : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform pointToSpawn;
        [SerializeField] private ItemData item;
        
        private ItemPoolManager _itemPoolManager;

        [Inject]
        private void Construct(NetworkManager networkManager)
        {
            _itemPoolManager = networkManager.GetComponent<ItemPoolManager>();
        }
        
        public void Interact()
        {
            var itemToDrop = _itemPoolManager.GetFromPool(item.ID);
        
            itemToDrop.transform.position = pointToSpawn.position;
            itemToDrop.SetActive(true);
        
            NetworkServer.Spawn(itemToDrop);
        }

        public void SrbToggle()
        {
            
        }
    }
}