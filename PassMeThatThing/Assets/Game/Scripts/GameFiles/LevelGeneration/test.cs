using System.Collections.Generic;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class test : MonoBehaviour
    {
        void Start()
        {
            // 1. Создаем тестовую витрину ивентов
            var eventsPool = new List<EventRoomDefinition>
            {
                new EventRoomDefinition("flood_1", GameEventsType.FloodBrokenPump, 10),
                new EventRoomDefinition("flood_2", GameEventsType.FloodPipeBreak, 10),
                new EventRoomDefinition("blackout_1", GameEventsType.BlackoutBlowFuse, 15),
                new EventRoomDefinition("other_1", GameEventsType.OtherEvent, 5)
            };

            // 2. Настраиваем макро-данные: 10 боковых комнат, 3 линии обороны, 100 бюджета
            var macroData = new LevelMacroData(
                totalRoomsWithoutHub: 10, 
                exitsCount: 1, 
                defenseRoomsCount: 3, 
                eventRoomsBudget: 100, 
                availableEventsPool: eventsPool
            );

            // 3. Генерируем граф
            var graphBuilder = new LevelGraphBuilder();
            var rootNode = graphBuilder.BuildGraph(macroData);

            // 4. Печатаем результат
            Debug.Log(graphBuilder.GetGraphStructureString(rootNode));
        }
    }
}