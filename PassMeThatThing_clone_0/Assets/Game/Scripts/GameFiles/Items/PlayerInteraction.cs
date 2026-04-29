using System;

namespace Game.Scripts.GameFiles.Items
{
    using UnityEngine;
    using Mirror;

    public class PlayerInteraction : NetworkBehaviour
    {
        public float interactionDistance = 3f;
        public LayerMask itemLayer; // Слой, на котором лежат предметы
        public Camera playerCamera;
    
        private PlayerInventory inventory;

        void Start()
        {
            inventory = GetComponent<PlayerInventory>();
            playerCamera = Camera.main;
        }

        void Update()
        {
            // Ввод обрабатываем ТОЛЬКО для локального игрока
            if (!isLocalPlayer) return;

            if (Input.GetKeyDown(KeyCode.E)) // Поднять
            {
                TryPickUp();
            }

            if (Input.GetKeyDown(KeyCode.Q)) // Выкинуть активный предмет
            {
                inventory.CmdDropItem(0); // Например, выкидываем первый слот
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 2);
        }

        void TryPickUp()
        {
            var targetsInRadius = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(transform.position,
                3, targetsInRadius, itemLayer);
            
            if (size >0)
            {
                if (targetsInRadius[0].TryGetComponent(out NetworkItem item))
                {
                    // Посылаем команду серверу, передавая NetworkIdentity предмета
                    Debug.Log("Trying to pick up an item");
                    inventory.CmdPickUpItem(item.gameObject);
                }
            }
        }
    }
}