using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments
{
    public class RoomController : MonoBehaviour
    {
        public int RoomId { get; private set; } = -1;
        public bool IsPowerOn { get; private set; } = true;

        private readonly List<OutlineShader> _lights = new();
        public IReadOnlyList<OutlineShader> Lights => _lights;

        // ВАЖНО: должен вызываться генератором уровня сразу после создания комнаты,
        // до Start(). ID обязан быть одинаковым на сервере и у всех клиентов —
        // то есть генерация должна быть детерминированной (один сид/один порядок).
        public void SetRoomId(int id)
        {
            RoomId = id;
        }

        private void Start()
        {
            if (RoomId < 0)
                Debug.LogWarning($"RoomId не назначен для {gameObject.name} — вызовите SetRoomId() из генератора уровня.");

            NetworkVisionManager.Instance.RegisterRoomLocal(RoomId, this);
        }

        public void RegisterLight(OutlineShader roomLight)
        {
            if (_lights.Contains(roomLight)) return;
            _lights.Add(roomLight);
            roomLight.SetActiveLocal(IsPowerOn);
        }

        public void UnregisterLight(OutlineShader roomLight) => _lights.Remove(roomLight);

        public void ApplyPowerState(bool state)
        {
            IsPowerOn = state;
            foreach (var light in _lights.Where(l => l != null))
                light.SetActiveLocal(state);
        }

        public void RequestSetPower(bool state)
        {
            if (RoomId < 0) return;
            NetworkVisionManager.Instance.SetRoomPower(RoomId, state);
        }

        private void OnDestroy()
        {
            if (NetworkVisionManager.Instance != null)
                NetworkVisionManager.Instance.UnregisterRoomLocal(RoomId);
        }
    }
}