using Assets.Game.Scripts.Enums;
using AYellowpaper.SerializedCollections;
using Game.Scripts.GameFiles.Items;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.Gameplay.View.OnScreenHints
{
    [CreateAssetMenu(fileName = "ScreenHintsDatabase", menuName = "Scriptable Objects/ScreenHintsDatabase")]
    public class ScreenHintsDatabase : ScriptableObject
    {
        [Header("иконки подсказок")]
        [SerializeField] public SerializedDictionary<UseHintType, Sprite> hints;

        public Sprite GetHintIcon(UseHintType type)
        {
            return hints[type];
        }
    }
}