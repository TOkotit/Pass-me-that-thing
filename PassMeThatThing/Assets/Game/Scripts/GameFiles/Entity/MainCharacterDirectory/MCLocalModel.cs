using Assets.Game.Scripts.Enums;
using Game.Scripts.Enums;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entity
{
    public class MCLocalModel
    {
        private int _health;
        private int _maxHealth;

        private bool _isDead;
        
        private ReactiveProperty<CursorViewType> _currentCursor = new();
        private ReactiveProperty<int> _cameraRotationSign = new();

        private PopupDescriptionMode _currentDescriptionMode;
        private string _currentInteractableText;
        private Dictionary<Resource, float> _itemRecycleResources = new();

        public event Action<int, int> OnHealthChanged;
        public event Action<bool> OnDeathChanged;
        public event Action<float> OnCameraYRotationChanged;
        public event Action<Vector3> OnCameraPositionChanged;
        public event Action<Vector3> OnPlayerPositionChanged;

        public event Action<PopupDescriptionMode> OnPopupDescriptionModeChanged;
        public event Action<string> OnCurrentInteractableTextChanged;
        public event Action<Dictionary<Resource, float>> OnItemRecycleResourcesChanged;

        public int Health
        {
            get => _health;
            set
            {
                OnHealthChanged?.Invoke(value, _maxHealth);
                _health = value;
            }
        }

        public int MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = value;
        }

        public bool IsDead
        {
            get => _isDead;
            set
            {
                if (_isDead != value)
                    OnDeathChanged?.Invoke(value);
                _isDead = value;
            }
        }

        public string CurrentInteractableText 
        { 
            get => _currentInteractableText;
            set
            {
                if (_currentInteractableText != value) 
                    OnCurrentInteractableTextChanged?.Invoke(value);
                _currentInteractableText = value;
            }
        }

        public ReactiveProperty<CursorViewType> CurrentCursor => _currentCursor;

        public ReactiveProperty<int> CameraRotationSign => _cameraRotationSign;

        public Dictionary<Resource, float> ItemRecycleResources
        { 
            get => _itemRecycleResources;
            set
            {
                OnItemRecycleResourcesChanged?.Invoke(value);
                _itemRecycleResources = value;
            }
        }

        public PopupDescriptionMode CurrentDescriptionMode
        { 
            get => _currentDescriptionMode;
            set 
            { 
                OnPopupDescriptionModeChanged?.Invoke(value);
                _currentDescriptionMode = value; 
            }
        }

        public void ReportCameraRotation(float angleY)
        {
            OnCameraYRotationChanged?.Invoke(angleY);
        }

        public void ReportCameraPosition(Vector3 value)
        {
            OnCameraPositionChanged?.Invoke(value);
        }
        
        public void ReportPlayerPosition(Vector3 value)
        {
            OnPlayerPositionChanged?.Invoke(value);
        }
    }
}