using Assets.Game.Scripts.GameFiles.Entity.Buildings.Misc;
using System;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class MainResourceStorage : ResourceStorage
    {
        private static ResourceStorage _instance;
        public static ResourceStorage Instance => _instance;

        [Inject] private LocalMainStorageModel _localMainStorageModel;

        public override void Awake()
        {
            base.Awake();
            if (!_instance)
            {
                _instance = this;
            }

            
        }

        public void Start()
        {
            _instance.OnAddedRes += _localMainStorageModel.InvokeOnAddedRes;
        }

        public void OnDestroy()
        {
            _instance.OnAddedRes -= _localMainStorageModel.InvokeOnAddedRes;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!_instance)
            {
                _instance = this;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (!_instance)
            {
                _instance = this;
            }
        }
    }
}