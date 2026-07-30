using System;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class BuildingHandler : NetworkBehaviour
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private GameObject defaultBuildingPreview; 
        [SerializeField] private Camera camera;

        [Inject] private LocalBuildingHandlerModel _handlerModel;
        [Inject] private BuildingManager _buildingManager;
        [Inject] private GlobalInventoryManager _globalInventoryManager;
        [Inject] private BuildingsDatabase _buildingDatabase;
        
        [Inject] private GameInputManager  _inputManager;
        [Inject] private GameplayUIManager _gameplayUIManager;
        

        private float _rotateStep = 3f;
        private float _previewRotation;
        private GameObject _buildingPreview;

        private float _previewDistance = 30f;
        
        private string _currentBuildingId;
        private bool _preview;

        private void Start()
        {
            if (isLocalPlayer)
            {

                _handlerModel.OnStartBuildPreviewById += StartBuildingPreviewById;
                _handlerModel.OnConfirmBuildPreview += ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview += CancelBuildingPreview;
                
                _inputManager.GameInput.Gameplay.Zoom.performed += ZoomOnperformed;
            }
        }


        private void OnDestroy()
        {
            if (isLocalPlayer)
            {

                _handlerModel.OnStartBuildPreviewById -= StartBuildingPreviewById;
                _handlerModel.OnConfirmBuildPreview -= ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview -= CancelBuildingPreview;
                
                _inputManager.GameInput.Gameplay.Zoom.performed -= ZoomOnperformed;
            }
        }


        private void ZoomOnperformed(InputAction.CallbackContext obj)
        {
            if (_buildingPreview != null)
            {
                //buildingPreview.transform.Rotate(new Vector3(0f, 1f, 0f), _rotateStep * obj.ReadValue<Vector2>().y); 
                _previewRotation += _rotateStep * obj.ReadValue<Vector2>().y;
            }
            
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            if (_preview)
            {
                var ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                
                if (Physics.Raycast(ray, out var hit, _previewDistance, groundLayer))
                {
                    _buildingPreview.transform.position = hit.point;


                    _buildingPreview.transform.rotation 
                        = Quaternion.FromToRotation(Vector3.up, hit.normal)
                        * Quaternion.AngleAxis(_previewRotation, Vector3.up);

                }
                // else
                // {
                //     buildingPreview.transform.position = ray.origin + ray.direction * _previewDistance;
                // }
            }
        }

        public void OpenBuildingPreviewScreen()
        {
            _gameplayUIManager.OpenScreenBuild();
        }

        public void CloseBuildingPreviewScreen()
        {
            _gameplayUIManager.OpenScreenGameplay();
        }
        
        public void StartBuildingPreviewById(string buildingId)
        {
            _preview = true;
            enabled = true;
            _currentBuildingId = buildingId;

            var prevPrefab = _buildingDatabase.GetBuildingFromAll(buildingId).previewPrefab;

            if (prevPrefab != null)
                _buildingPreview = Instantiate(prevPrefab);
            else
            {
                _buildingPreview = Instantiate(defaultBuildingPreview);
            }

            //buildingPreview.SetActive(true);
            OpenBuildingPreviewScreen();
        }

        public void ConfirmBuilding()
        {
            var ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            var rotation = Quaternion.identity;
            var position = new Vector3();
            
            if (Physics.Raycast(ray, out var hit, _previewDistance, groundLayer))
            {
                position = hit.point;

                rotation = _buildingPreview.transform.rotation;
            }
            
            _buildingManager.CmdSpawnBuilding(position, rotation, _currentBuildingId);

            if (!string.IsNullOrEmpty(_handlerModel.InstanceId))
            {
                _globalInventoryManager.CmdDeleteFromInventory(_handlerModel.InstanceId);
                CancelBuildingPreview();
            }
        }
        
        public void CancelBuildingPreview()
        {
            _preview = false;
            enabled = false;
            _currentBuildingId = "";
            if (_buildingPreview != null)
                Destroy(_buildingPreview.gameObject);
            //buildingPreview.SetActive(false);
            CloseBuildingPreviewScreen();
        }
        
    }
}