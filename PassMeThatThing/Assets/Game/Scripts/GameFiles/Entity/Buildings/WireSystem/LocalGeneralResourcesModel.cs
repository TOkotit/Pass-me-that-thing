using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    /// <summary>
    /// общие ресы бункера с разных мест: эл-во, вода и топливо
    /// </summary>
    public class LocalGeneralResourcesModel
    {
        private Dictionary<WireType, WireNetNetworkData> _res = new();
        public Dictionary<WireType, WireNetNetworkData> Res => _res;


        public event Action OnResChanged;

        public WireNetNetworkData GetVal(WireType wireType)
        {
            if (_res.TryGetValue(wireType, out var val)) return val;
            return new WireNetNetworkData();
        }

        public void ResChanged() => OnResChanged?.Invoke();

    }
}
