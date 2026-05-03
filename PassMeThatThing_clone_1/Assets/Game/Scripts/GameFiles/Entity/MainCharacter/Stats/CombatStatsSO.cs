using UnityEngine;

namespace MainCharacter
{
    [CreateAssetMenu(fileName = "CombatStats", menuName = "MainCharacter/CombatStatsSO")]
    public class CombatStatsSO : ScriptableObject
    {
        [SerializeField] private float parryReloadDelay;
        [SerializeField] private float parryDuration;
        public float ParryReloadDelay => parryReloadDelay;
        public float ParryDuration => parryDuration;
    }
}