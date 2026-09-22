
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Entity.MainCharacterDirectory.Stats
{
    /// <summary>
    /// Визуальная инфа о классе, основная инфа остается в ClassStats
    /// id должен совпадать
    /// </summary>
    [CreateAssetMenu(fileName = "ClassData", menuName = "Scriptable Objects/ClassData")]
    public class ClassData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string description;

        [SerializeField] private Sprite image;
        [SerializeField] private Color color;

        public string Id => id;
        public string Description => description;
        public Sprite Image => image;
        public Color Color => color;
    }
}