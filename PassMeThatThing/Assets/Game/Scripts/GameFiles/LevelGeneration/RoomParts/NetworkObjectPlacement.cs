using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class NetworkObjectPlacement : NetworkBehaviour
    {
        [SyncVar]
        private int _spotIndex = -1;

        public int SpotIndex => _spotIndex;

        public void SetSpotIndex(int spotIndex)
        {
            if (!isServer)
                return;

            _spotIndex = spotIndex;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (NetworkObjectsOrchestrator.Instance != null)
            {
                NetworkObjectsOrchestrator.Instance.RegisterPendingObject(this);
            }
            else
            {
                Debug.LogWarning(
                    $"[NETWORK] NetworkObjectsOrchestrator.Instance is null for {name}.");
            }
        }

        public override void OnStopClient()
        {
            if (NetworkObjectsOrchestrator.Instance != null)
            {
                NetworkObjectsOrchestrator.Instance.UnregisterPendingObject(this);
            }

            base.OnStopClient();
        }
    }
}