using FishNet.Object;
using FishNet.Object.Synchronizing;

public class NetworkRoomPlayer : NetworkBehaviour
{
    public readonly SyncVar<string> PlayerName = new("Player");
    public readonly SyncVar<bool> IsReady = new SyncVar<bool>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        CustomRoomManager.Instance.RegisterRoomPlayer(this);

        PlayerName.OnChange += OnNameChanged;
        IsReady.OnChange += OnReadyChanged;
        
        // Вызываем один раз при старте, чтобы отобразить текущие данные
        CustomRoomManager.Instance.RefreshGUI();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (CustomRoomManager.Instance != null)
            CustomRoomManager.Instance.UnregisterRoomPlayer(this);

        // Обязательно отписываемся от событий во избежание утечек памяти
        PlayerName.OnChange -= OnNameChanged;
        IsReady.OnChange -= OnReadyChanged;
    }

    [ServerRpc]
    public void CmdSetReady(bool ready)
    {
        // Изменение значения SyncVar<> на сервере происходит через .Value
        IsReady.Value = ready;
        CustomRoomManager.Instance.CheckStartCondition();
    }

    [ServerRpc]
    public void CmdSetName(string name)
    {
        PlayerName.Value = name;
    }

    // Сигнатура метода OnChange в FishNet принимает: (T prev, T next, bool asServer)
    private void OnNameChanged(string prev, string next, bool asServer) => CustomRoomManager.Instance.RefreshGUI();
    private void OnReadyChanged(bool prev, bool next, bool asServer) => CustomRoomManager.Instance.RefreshGUI();
}