using System;
using Game.Gameplay.View.UI;
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
        [SerializeField] private GameObject buildingPreview;
        [SerializeField] private Camera camera;

        [Inject] private LocalBuildingHandlerModel _handlerModel;
        [Inject] private BuildingManager _buildingManager;
        [Inject] private GameInputManager  _inputManager;
        [Inject] private GameplayUIManager _gameplayUIManager;
        
        private float _maxDistance = 15f;
        private float _minDistance = 3f;
        private float _zoomstep = 0.5f;
        
        private float _previewDistance = 30f;
        
        private string _currentBuildingId;
        private bool _preview;

        private void Start()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnStartBuildPreviewByIndex += StartBuildingPreviewByIndex;
                _handlerModel.OnStartBuildPreviewById += StartBuildingPreviewById;
                _handlerModel.OnConfirmBuildPreview += ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview += CancelBuildingPreview;
                
                // _inputManager.GameInput.Gameplay.Zoom.performed += ZoomOnperformed;
            }
        }


        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnStartBuildPreviewByIndex -= StartBuildingPreviewByIndex;
                _handlerModel.OnStartBuildPreviewById -= StartBuildingPreviewById;
                _handlerModel.OnConfirmBuildPreview -= ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview -= CancelBuildingPreview;
                
                // _inputManager.GameInput.Gameplay.Zoom.performed -= ZoomOnperformed;
            }
        }

        
        // private void ZoomOnperformed(InputAction.CallbackContext obj)
        // {
        //     _previewDistance = Mathf.Clamp(_previewDistance + _zoomstep * obj.ReadValue<Vector2>().y, _minDistance, _maxDistance);
        // }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            if (_preview)
            {
                var ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                
                if (Physics.Raycast(ray, out var hit, _previewDistance, groundLayer))
                {
                    buildingPreview.transform.position = hit.point;
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
        

        public void StartBuildingPreviewByIndex(int buildingIndex)
        {
            _preview = true;
            enabled = true;
            // _currentBuildingIndex = buildingIndex;
            buildingPreview.SetActive(true);
            OpenBuildingPreviewScreen();
        }
        
        public void StartBuildingPreviewById(string buildingId)
        {
            _preview = true;
            enabled = true;
            _currentBuildingId = buildingId;
            buildingPreview.SetActive(true);
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

                rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            // else
            // {
            //     position = ray.origin + ray.direction * _previewDistance;
            // }
            
            _buildingManager.CmdSpawnBuilding(position, rotation, _currentBuildingId);

        }
        
        public void CancelBuildingPreview()
        {
            _preview = false;
            enabled = false;
            _currentBuildingId = "";
            buildingPreview.SetActive(false);
        }
        
    }
}