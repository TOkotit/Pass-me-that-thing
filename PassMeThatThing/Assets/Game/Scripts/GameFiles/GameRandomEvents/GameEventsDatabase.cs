using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEventsDatabase", menuName = "Scriptable Objects/GameEventsDatabase")]
public class GameEventsDatabase : ScriptableObject
{
    public List<GameEventData> allEvents;
    
    public GameEventData GetEvent(GameEventsType type)
    {
        return allEvents.Find(e => e.GameEventType == type);
    }
}
