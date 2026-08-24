using System;
using Assets.Game.Scripts.GameFiles.Entity.Buildings;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.Entity.Buildings.Misc.Craft;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using R3;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class BuildingHandler : NetworkBehaviour
    {
        [SerializeField] private float rotateStep = 3f;
        [SerializeField] private float previewDistance = 30f;

        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask ceilingLayer;
        [SerializeField] private LayerMask wallsLayer;

        [SerializeField] private Material defaultPreviewMat;
        [SerializeField] private Material collisionPreviewMat;

        [SerializeField] private LayerMask buildingLayer;
        [SerializeField] private GameObject defaultBuildingPreview; 
        [SerializeField] private Camera camera;

        [Inject] private LocalBuildingHandlerModel _handlerModel;
        [Inject] private BuildingManager _buildingManager;
        [Inject] private GlobalInventoryManager _globalInventoryManager;
        [Inject] private BuildingsDatabase _buildingDatabase;
        
        [Inject] private GameInputManager  _inputManager;
        [Inject] private GameplayUIManager _gameplayUIManager;
        
        private string _currentBuildingId;
        private BuildingData _currentBuildingData;

        private GameObject _buildingPreview;
        private bool _preview;
        private float _previewRotation;

        private PreviewCollisionHandler _currentCollisionHandler;
        private LayerMask _collisionLayer;
        private ReactiveProperty<bool> _isCollided = new();
        private CompositeDisposable _subs = new();

        private void Start()
        {
            _collisionLayer = groundLayer | ceilingLayer | wallsLayer;
            if (isLocalPlayer)
            { 
                _handlerModel.OnStartBuildPreviewById += StartBuildingPreviewById;
                _handlerModel.OnConfirmBuildPreview += ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview += CancelBuildingPreview;

                _handlerModel.OnDestroyBuilding += DestroyBuilding;
                
                _inputManager.GameInput.Gameplay.Zoom.performed += ZoomOnperformed;


                _subs.Add(_isCollided.Subscribe(ChangeMaterials));
            }
        }


        private void OnDestroy()
        {
            if (isLocalPlayer)
            {

                _handlerModel.OnStartBuildPreviewById -= StartBuildingPreviewById;
                _handlerModel.OnConfirmBuildPreview -= ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview -= CancelBuildingPreview;

                _handlerModel.OnDestroyBuilding -= DestroyBuilding;

                _inputManager.GameInput.Gameplay.Zoom.performed -= ZoomOnperformed;


                _subs.Dispose();
            }
        }


        private void ZoomOnperformed(InputAction.CallbackContext obj)
        {
            if (_buildingPreview != null)
            {
                switch (_currentBuildingData.rotationType)
                {
                    case Enums.BuildingRotationType.Locked:
                        break;
                    case Enums.BuildingRotationType.Free:
                        _previewRotation += rotateStep * obj.ReadValue<Vector2>().y;
                        break;
                    case Enums.BuildingRotationType.Free90Deg:
                        _previewRotation += 90f * Math.Sign(obj.ReadValue<Vector2>().y);
                        break;
                    case Enums.BuildingRotationType.Free180Deg:
                        _previewRotation += 180f;
                        break;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            if (_preview)
            {
                var ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                //TODO проверка на потолок стены пол
                if (Physics.Raycast(ray, out var hit, previewDistance, groundLayer))
                {
                    _buildingPreview.transform.position = hit.point;

                    //_buildingPreview.transform.rotation
                    //    = Quaternion.FromToRotation(Vector3.up, hit.normal)

                    //    * Quaternion.AngleAxis(_previewRotation, Vector3.up);
                    _buildingPreview.transform.rotation
                        =
                        Quaternion.LookRotation(hit.normal, Vector2.up)

                        * Quaternion.AngleAxis(_previewRotation, hit.normal);


                    if (_currentBuildingData.isCollisionChecking)
                    {
                        _isCollided.Value = Physics.CheckBox(_currentCollisionHandler.BoxCenter.position,
                        _currentCollisionHandler.BoxHalfExtends,
                        _buildingPreview.transform.rotation,
                        _collisionLayer);
                    }
                }
            }
        }

        private void ChangeMaterials(bool v)
        {
            foreach (var r in _currentCollisionHandler.Renderers)
            {
                var currentMaterials = r.materials;
                var newMaterialsArray = new Material[currentMaterials.Length];

                for (int i = 0; i < newMaterialsArray.Length; i++)
                {
                    newMaterialsArray[i] = v ? collisionPreviewMat : defaultPreviewMat;
                }

                r.materials = newMaterialsArray;
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
            _currentBuildingId = buildingId;
            _currentBuildingData = _buildingDatabase.GetBuildingFromAll(buildingId);
            _preview = true;
            enabled = true;

            if (_currentBuildingData.rotationType == Enums.BuildingRotationType.Locked)
                _previewRotation = 0f;

            var prevPrefab = _currentBuildingData.previewPrefab;

            if (prevPrefab != null)
                _buildingPreview = Instantiate(prevPrefab);
            else
                _buildingPreview = Instantiate(defaultBuildingPreview);

            if (_currentBuildingData.isCollisionChecking)
            {
                _buildingPreview.TryGetComponent(out _currentCollisionHandler);
            }

            OpenBuildingPreviewScreen();
        }

        public void ConfirmBuilding()
        {
            //var ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            var rotation = _buildingPreview.transform.rotation;
            var position = _buildingPreview.transform.position;
            
            //if (Physics.Raycast(ray, out var hit, previewDistance, groundLayer))
            //{
            //    position = hit.point;

            //    rotation = _buildingPreview.transform.rotation;
            //}

            if (_currentBuildingData.isCollisionChecking
                && _isCollided.Value) return;
                

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
            _currentBuildingData = null;
            if (_buildingPreview != null)
                Destroy(_buildingPreview.gameObject);
            CloseBuildingPreviewScreen();
        }
        
        public void DestroyBuilding()
        {
            var ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            if (Physics.Raycast(ray, out var hit, previewDistance, buildingLayer))
            {
                _buildingManager.CmdDestroyBuilding(hit.collider.gameObject);
            }
        }

    }
}