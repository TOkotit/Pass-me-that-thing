using System;
using Game.Gameplay.View.UI;
using Mirror;
using UnityEngine;
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
        [Inject] private GameplayUIManager _gameplayUIManager;
        
        private int _currentBuildingIndex;
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
            }
        }


        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            if (_preview)
            {
                var ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                if (Physics.Raycast(ray, out var _hit, 50f, groundLayer))
                {
                    buildingPreview.transform.position = _hit.point;
                }
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
            _currentBuildingIndex = buildingIndex;
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
            if (_currentBuildingId == "")
            {
                _buildingManager.CmdSpawnBuilding(buildingPreview.transform.position, _currentBuildingIndex);
            }
            else
            {
                _buildingManager.CmdSpawnBuilding(buildingPreview.transform.position, _currentBuildingId);
            }
            CancelBuildingPreview();
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