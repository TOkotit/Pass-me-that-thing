using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Game.Scripts.GameFiles.Items;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Game.Entity.Stats;
using Assets.Game.Scripts.GameFiles.Entity.MainCharacterDirectory.Stats;
using UnityEngine.UI;

namespace Assets.Game.Scripts.GameFiles.InteractableObjects
{
    public class ClassShelfObject : NetworkBehaviour, Interactable
    {
        [SerializeField] private ClassData classData;

        [SerializeField] private Outline outline;

        [SyncVar]
        private bool _isEnabled;

        public bool IsEnabled => _isEnabled;
        public ClassData ClassData => classData;


        public event Action<string> OnClassChanged;

        public void Interact()
        {
            if (_isEnabled)
            {
                OnClassChanged?.Invoke(classData.Id);
            }
        }

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        public void SrbToggle() { }

        public void InteractWithItem(PhysicalItem item) { }

        public override void OnStartClient()
        {
            base.OnStartClient();
            outline.OutlineColor = ClassData.Color;
            InteractableRegistry.Instance.Register(gameObject, this);
            if (!InteractableRegistry.Instance.TryGetInteractable(gameObject, out Interactable interactable))
            {
                Debug.LogError("Interactable not found");
            }
        }
    }
}
