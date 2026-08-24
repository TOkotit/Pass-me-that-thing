using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription;
using Game.Entity;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.Items
{
    public class PlayerInteractableDescriptionHandler : MonoBehaviour
    {
        [SerializeField] private float interval = 0.3f;
        [SerializeField] private Camera playerCamera;

        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private float interactionDistance;

        [Inject] private WireManager _wiremanager;
        [Inject] private MCLocalModel _localModel;
        [Inject] private PhysicalItemRegistry _physicalItemRegistry;
        [Inject] private GameplayUIManager _gameplayUIManager;

        private WindowDescriptionViewModel _currentDescription;
        private Transform _currentTransform;
        private float _timer;


        private void Start()
        {
            _currentDescription = _gameplayUIManager.OpenWindowDescription();
            _currentDescription.enabled.Value = false;
        }
        private void OnDestroy()
        {
            _gameplayUIManager.CloseWindowDescription(_currentDescription);
        }

        private void FixedUpdate()
        {
            _timer += Time.fixedDeltaTime;

            if (_timer >= interval)
            {
                GetDescription();
                _timer = 0f;
            }
        }

        private void GetDescription()
        {
            var ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            if (Physics.Raycast(ray, out var hit, interactionDistance, interactionLayer))
            {
                if (hit.collider.gameObject.transform == _currentTransform) return;
                _currentTransform = hit.collider.gameObject.transform;

                if (hit.collider.gameObject.CompareTag("Item"))
                {
                    OpenWindow();

                    var item = _physicalItemRegistry.GetItem(hit.collider.gameObject);
                    _localModel.CurrentInteractableText = item.Network.ItemData.Id;
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
                    _currentTransform = null;
                    _currentDescription.enabled.Value = false;
                }
            }
            else
            {
                _currentTransform = null;
                _currentDescription.enabled.Value = false;
            }
        }

        private void OpenWindow()
        {
            _currentDescription.parentPos = _currentTransform.position;
            _currentDescription.enabled.Value = true;
        }

    }
}