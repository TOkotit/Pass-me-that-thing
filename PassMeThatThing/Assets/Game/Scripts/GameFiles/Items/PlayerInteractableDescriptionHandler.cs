using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.PopupDescription;
using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription;
using Entity;
using Game.Entity;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Items
{
    //TODO мб потом сделать не на игроке этот класс а камеру получать как то по другому
    public class PlayerInteractableDescriptionHandler : NetworkBehaviour
    {
        [SerializeField] private float interval = 0.3f;
        [SerializeField] private Camera playerCamera;

        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private LayerMask buildingLayer;
        [SerializeField] private float interactionDistance;

        [Inject] private WireManager _wiremanager;
        [Inject] private MCLocalModel _localModel;
        [Inject] private PhysicalItemRegistry _physicalItemRegistry;
        [Inject] private DamagableRegistry _damageableRegistry;
        [Inject] private GameplayUIManager _gameplayUIManager;

        private PopupDescriptionViewModel _currentDescription;
        private Transform _currentTransform;
        private float _timer;


        private void Start()
        {
            if (isLocalPlayer)
            {
                _currentDescription = _gameplayUIManager.OpenPopupDescription();
                _currentDescription.enabled.Value = false;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                _gameplayUIManager.ClosePopupDescription(_currentDescription);
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= interval)
            {
                GetDescription();
                _timer = 0f;
            }

            if (_currentDescription.enabled.Value)
            {
                _currentDescription.screenPos.Value 
                    = playerCamera.WorldToScreenPoint(_currentTransform.position);
            }
        }

        private void GetDescription()
        {
            var ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
            {
                if (hit.collider.gameObject.transform == _currentTransform) return;
                _currentTransform = hit.collider.gameObject.transform;

                if (hit.collider.gameObject.CompareTag("Item"))
                {
                    OpenWindow();

                    //var item = _physicalItemRegistry.GetItem(hit.collider.gameObject);
                    var item = hit.collider.gameObject.GetComponentInParent<PhysicalItem>();
                    if (item != null)
                    {
                        _localModel.CurrentInteractableText = item.Network.ItemData.Id;
                    }
                }
                else if (hit.collider.gameObject.CompareTag("Door"))
                {
                    OpenWindow();

                    _localModel.CurrentInteractableText = "Interact"; //заглушка
                }
                else if (hit.collider.gameObject.CompareTag("WireNode"))
                {
                    var wireNode = hit.collider.gameObject.GetComponentInParent<WireNode>();

                    if (wireNode.NetId == -1) return;

                    OpenWindow();

                    var net = _wiremanager.WireNetsData[wireNode.NetId];

                    _localModel.CurrentInteractableText = $"{net.availableQuantity}/{net.requiredQuantity}";
                }
                else
                {
                    CloseWindow();
                }
            }
            else if (Physics.Raycast(ray, out hit, interactionDistance, buildingLayer))
            {
                if (hit.collider.gameObject.transform == _currentTransform) return;
                _currentTransform = hit.collider.gameObject.transform;

                if (_damageableRegistry.TryGetDamagable(hit.collider.gameObject, out var dam))
                {
                    OpenWindow();

                    _localModel.CurrentInteractableText = $"{dam.DamagableModel.HealthPool.CurrentHealth}/{dam.DamagableModel.HealthPool.MaxHealth}";
                }
                else
                {
                    CloseWindow();
                }
            }
            else
            {
                CloseWindow();
            }
        }

        private void OpenWindow()
        {
            _currentDescription.enabled.Value = true;
        }

        private void CloseWindow()
        {
            _currentDescription.enabled.Value = false;
            _currentTransform = null;
        }

    }
}