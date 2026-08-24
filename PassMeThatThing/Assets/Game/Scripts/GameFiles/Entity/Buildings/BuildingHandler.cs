using System;
using System.Collections.Generic;
using Assets.Game.Scripts.GameFiles.Entity.Buildings;
using Game.Gameplay.View.UI;
using Game.Scripts.Enums;
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

        [SerializeField] private string floorTag;
        [SerializeField] private string ceilingTag;
        [SerializeField] private string wallTag;

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

        private TagHandle _floorTag;
        private TagHandle _ceilingTag;
        private TagHandle _wallTag;

        private string _currentBuildingId;
        private BuildingData _currentBuildingData;

        private GameObject _buildingPreview;
        private bool _preview;
        private float _previewRotation;

        private List<TagHandle> _currentBuildingPlacementTags = new();
        private bool _isPlaced = new();

        private PreviewBuildingHandler _currentBuildingHandler;
        private LayerMask _collisionLayer;
        private bool _isCollided;

        private ReactiveProperty<bool> _isBuildValid = new();
        private CompositeDisposable _subs = new();

        private void Awake()
        {
            _floorTag = TagHandle.GetExistingTag(floorTag);
            _wallTag = TagHandle.GetExistingTag(wallTag);
            _ceilingTag = TagHandle.GetExistingTag(ceilingTag);

            _collisionLayer = groundLayer;
        }

        private void Start()
        {
            if (isLocalPlayer)
            { 
                _handlerModel.OnStartBuildPreviewById += StartBuildingPreviewById;
                _handlerModel.OnConfirmBuildPreview += ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview += CancelBuildingPreview;

                _handlerModel.OnDestroyBuilding += DestroyBuilding;
                
                _inputManager.GameInput.Gameplay.Zoom.performed += ZoomOnperformed;


                _subs.Add(_isBuildValid.Subscribe(ChangeMaterials));
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

                if (Physics.Raycast(ray, out var hit, previewDistance, _collisionLayer))
                {
                    _buildingPreview.transform.position = hit.point;

                    _buildingPreview.transform.rotation
                        = Quaternion.AngleAxis(_previewRotation, hit.normal)
                        * Quaternion.LookRotation(hit.normal, Vector3.up);

                    _isPlaced = false;
                    foreach (var t in _currentBuildingPlacementTags)
                    {
                        if (hit.collider.gameObject.CompareTag(t))
                        {
                            _isPlaced = true;
                            break;
                        }
                    }

                    if (_currentBuildingData.isCollisionChecking)
                    {
                        _isCollided = Physics.CheckBox(_currentBuildingHandler.BoxCenter.position,
                        _currentBuildingHandler.BoxHalfExtends,
                        _buildingPreview.transform.rotation,
                        _collisionLayer);

                        _isBuildValid.Value = _isPlaced && !_isCollided;
                    }
                    else
                    {
                        _isBuildValid.Value = _isPlaced;
                    }
                }
            }
        }

        private void ChangeMaterials(bool v)
        {
            foreach (var r in _currentBuildingHandler.Renderers)
            {
                var currentMaterials = r.materials;
                var newMaterialsArray = new Material[currentMaterials.Length];

                for (int i = 0; i < newMaterialsArray.Length; i++)
                {
                    newMaterialsArray[i] = v ? defaultPreviewMat : collisionPreviewMat;
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
            
            //спавн
            var prevPrefab = _currentBuildingData.previewPrefab;
            if (prevPrefab != null)
                _buildingPreview = Instantiate(prevPrefab);
            else
                _buildingPreview = Instantiate(defaultBuildingPreview);

            //вращение
            if (_currentBuildingData.rotationType == Enums.BuildingRotationType.Locked)
                _previewRotation = 0f;

            //расположение
            _currentBuildingPlacementTags.Clear();
            if (_currentBuildingData.placementType.HasFlag(BuildingPlacementType.Floor))
                _currentBuildingPlacementTags.Add(_floorTag);

            if (_currentBuildingData.placementType.HasFlag(BuildingPlacementType.Walls))
                _currentBuildingPlacementTags.Add(_wallTag);

            if (_currentBuildingData.placementType.HasFlag(BuildingPlacementType.Ceiling))
                _currentBuildingPlacementTags.Add(_ceilingTag);

            //коллизии и расположение
            _buildingPreview.TryGetComponent(out _currentBuildingHandler);

            OpenBuildingPreviewScreen();
        }

        public void ConfirmBuilding()
        {
            if (!_isBuildValid.Value) return;

            var rotation = _buildingPreview.transform.rotation;
            var position = _buildingPreview.transform.position;

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