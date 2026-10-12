using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEventsDatabase", menuName = "Scriptable Objects/GameEventsDatabase")]
public class GameEventsDatabase : ScriptableObject
{
    public List<GameEventData> allEvents;

    [Header("иконки типов задач")]
    public SerializedDictionary<GameTaskType, Sprite> taskTypeIcons;
    
    public GameEventData GetEvent(GameEventsType type)
    {
        return allEvents.Find(e => e.GameEventType == type);
    }
}
