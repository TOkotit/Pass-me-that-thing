using System;

namespace Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class MainResourceStorage : ResourceStorage
    {
        private static ResourceStorage _instance;
        public static ResourceStorage Instance => _instance;

        private void Awake()
        {
            if (!_instance)
            {
                _instance = this;
            }
        }
    }
}