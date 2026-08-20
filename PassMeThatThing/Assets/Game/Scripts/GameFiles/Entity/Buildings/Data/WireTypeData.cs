using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.Buildings.Data
{
    [CreateAssetMenu(fileName = "WireTypeData", menuName = "Scriptable Objects/WireTypeData")]
    public class WireTypeData : ScriptableObject
    {
        public string wireTypeName;
        public Sprite wireTypeImage;
    }
}