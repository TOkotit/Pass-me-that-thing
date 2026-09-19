using System;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class MainResourceStorage : ResourceStorage
    {
        private static ResourceStorage _instance;
        public static ResourceStorage Instance => _instance;

        public override void Awake()
        {
            base.Awake();
            if (!_instance)
            {
                _instance = this;
            }
        }

        public override void OnStartClient()
        {
            base.Awake();
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