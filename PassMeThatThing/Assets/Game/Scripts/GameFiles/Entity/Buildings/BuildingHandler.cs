using System;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings
{
    public class BuildingHandler : NetworkBehaviour
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private GameObject buildingPreview;

        [Inject] private LocalBuildingHandlerModel _handlerModel;
        [Inject] private BuildingManager _buildingManager;
        
        private int _currentBuildingIndex;
        private bool _preview;

        private void Start()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnStartBuildPreview += StartBuildingPreview;
                _handlerModel.OnConfirmBuildPreview += ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview += CancelBuildingPreview;
            }
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                _handlerModel.OnStartBuildPreview -= StartBuildingPreview;
                _handlerModel.OnConfirmBuildPreview -= ConfirmBuilding;
                _handlerModel.OnCancelBuildPreview -= CancelBuildingPreview;
            }
        }


        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            if (_preview)
            {
                var _ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(_ray, out var _hit, 50f, groundLayer))
                {
                    buildingPreview.transform.position = _hit.point;
                }
            }
        }

        public void StartBuildingPreview(int buildingIndex)
        {
            _preview = true;
            enabled = true;
            _currentBuildingIndex = buildingIndex;
            buildingPreview.SetActive(true);
        }

        public void ConfirmBuilding()
        {
            _buildingManager.CmdSpawnBuilding(buildingPreview.transform.position, _currentBuildingIndex);
            CancelBuildingPreview();
        }
        
        public void CancelBuildingPreview()
        {
            _preview = false;
            enabled = false;
            buildingPreview.SetActive(false);
        }
        
    }
}