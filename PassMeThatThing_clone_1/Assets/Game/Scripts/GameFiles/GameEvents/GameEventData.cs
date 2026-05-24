using Game.Scripts.Enums;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameEventData", menuName = "Scriptable Objects/GameEventData")]
public class GameEventData : ScriptableObject
{
    [SerializeField] private GameEventsType gameEventType;
    
    [SerializeField] private string eventName;
    [SerializeField] private Sprite eventImage;

    public Sprite EventImage
    {
        get => eventImage;
        set => eventImage = value;
    }

    public string EventName
    {
        get => eventName;
        set => eventName = value;
    }

    public GameEventsType GameEventType
    {
        get => gameEventType;
        set => gameEventType = value;
    }
}
