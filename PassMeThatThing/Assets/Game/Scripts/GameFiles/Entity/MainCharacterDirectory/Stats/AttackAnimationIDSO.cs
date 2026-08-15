using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Entity.Stats
{
    [CreateAssetMenu(fileName = "AttackAnimationIdSO", menuName = "Stats/AttackAnimationIdSO")]
    public class AttackAnimationIdSO : ScriptableObject
    {
        [SerializedDictionary]
        public SerializedDictionary<AttackAnimationType, string> attackIds =
            new SerializedDictionary<AttackAnimationType, string>()
            {
                { AttackAnimationType.AttackVertical1, "AttackVertical1" },
                { AttackAnimationType.AttackHorizontal1, "AttackHorizontal1" }
            };
    }
}