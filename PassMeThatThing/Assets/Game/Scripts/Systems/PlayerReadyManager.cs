using System.Collections.Generic;
using Mirror;
using System;

public class PlayerReadyManager
{
    private List<NetworkIdentity> players = new List<NetworkIdentity>();
    private HashSet<NetworkIdentity> readyPlayers = new HashSet<NetworkIdentity>();
    public event Action OnAllPlayersReady;

    public void Register(NetworkIdentity player)
    {
        if (!player) return;
        if (!players.Contains(player))
            players.Add(player);
    }

    public void SetReady(NetworkIdentity player)
    {
        if (!player || !players.Contains(player)) return;
        readyPlayers.Add(player);

        if (readyPlayers.Count == players.Count && players.Count > 0)
            OnAllPlayersReady?.Invoke();
    }

    public void ResetReady()
    {
        readyPlayers.Clear();
    }

    public void ClearAll()
    {
        players.Clear();
        readyPlayers.Clear();
    }
}