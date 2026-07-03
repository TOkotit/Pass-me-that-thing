using UnityEngine;

namespace Game.Scripts.Systems
{
    public interface IJsonSaveable
    {
        public void SaveToJson();
        public void LoadFromJson();
    }
}