using Game.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Misc
{
    public class LocalMainStorageModel
    {
        public event Action<Resource, float> OnAddedRes;

        public void InvokeOnAddedRes(Resource resource, float v)
        {
            OnAddedRes?.Invoke(resource, v);
        }
    }
}
