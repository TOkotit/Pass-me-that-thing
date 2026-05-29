using UnityEngine;

namespace MainCharacter_old
{
    [CreateAssetMenu(fileName = "CombatStats", menuName = "MainCharacter_old/CombatStatsSO")]
    public class CombatStatsSO : ScriptableObject
    {
        [SerializeField] private float parryReloadDelay;
        [SerializeField] private float parryDuration;
        public float ParryReloadDelay => parryReloadDelay;
        public float ParryDuration => parryDuration;
    }
}