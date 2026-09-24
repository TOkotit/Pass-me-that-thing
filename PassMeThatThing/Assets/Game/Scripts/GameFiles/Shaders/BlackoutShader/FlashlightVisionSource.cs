using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.Blackout
{
    public class FlashlightVisionSource : NetworkBehaviour
    {
        [SerializeField] private Light _light;

        // Синхронизируется по сети — это единственное, что реально нужно разослать другим игрокам.
        // Позиция/поворот фонарика уже приходят через NetworkTransform на объекте игрока (если он есть).
        [SyncVar]
        private bool _isOn = true;

        private void Awake()
        {
            if (!_light)
            {
                Debug.LogError($"[GameRandomEvents] No light found on {name}");
                return;
            };

            if (_light.type != LightType.Spot)
                Debug.LogWarning($"[FlashlightVisionSource] На {gameObject.name} Light не Spot — конус видимости может работать некорректно.");
        }

        private void Update()
        {
           
            if (!_isOn) return;
            if (!_light.enabled) return;
            if (GlobalVisionShaderManager.Instance == null) return;

            GlobalVisionShaderManager.Instance.AddConeZone(
                transform.position,
                transform.forward,
                _light.spotAngle * 0.5f,
                _light.range
            );
        }

        [Command]
        public void CmdSetFlashlightOn(bool state)
        {
            _isOn = state;
        }

        private void OnDrawGizmosSelected()
        {
            if (_light == null) _light = GetComponent<Light>();
            if (_light == null) return;

            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.4f);
            var halfAngleRad = _light.spotAngle * 0.5f * Mathf.Deg2Rad;
            var endRadius = Mathf.Tan(halfAngleRad) * _light.range;

            var endCenter = transform.position + transform.forward * _light.range;
            Gizmos.DrawLine(transform.position, endCenter);
            Gizmos.DrawWireSphere(endCenter, endRadius);
        }
    }
}