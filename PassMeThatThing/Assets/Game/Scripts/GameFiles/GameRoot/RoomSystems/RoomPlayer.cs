using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class NetworkRoomPlayer : NetworkBehaviour
{
    // // Синхронизируем имя и статус готовности со всеми клиентами
    // [SyncVar(OnChange = nameof(OnNameChanged))]
    // public string PlayerName = "Player";
    //
    // [SyncVar(OnChange = nameof(OnReadyChanged))]
    // public bool IsReady = false;
    //
    // public override void OnStartClient()
    // {
    //     base.OnStartClient();
    //     // Регистрируем игрока в локальном списке менеджера лобби
    //     RoomManager.Instance.RegisterPlayer(this);
    // }
    //
    // public override void OnStopClient()
    // {
    //     base.OnStopClient();
    //     RoomManager.Instance.UnregisterPlayer(this);
    // }
    //
    // // Этот метод вызывается на клиенте, чтобы сказать серверу "я готов"
    // [ServerRpc]
    // public void CmdSetReady(bool ready)
    // {
    //     IsReady = ready;
    //     RoomManager.Instance.CheckStartCondition();
    // }
    //
    // [ServerRpc]
    // public void CmdSetName(string name)
    // {
    //     PlayerName = name;
    // }
    //
    // private void OnNameChanged(string oldName, string newName, bool asServer) => RoomManager.Instance.RefreshGUI();
    // private void OnReadyChanged(bool oldReady, bool newReady, bool asServer) => RoomManager.Instance.RefreshGUI();
}