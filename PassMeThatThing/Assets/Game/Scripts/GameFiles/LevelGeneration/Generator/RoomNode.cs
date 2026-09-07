using System.Collections.Generic;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    /// <summary>
    /// <para>
    /// Абстрактный узел комнаты для генератора<br/>
    /// Пока хранит только тип комнаты <br/>
    /// </para>
    /// Используется в <see cref="LevelGenerator"/>
    /// </summary>
    public class RoomNode
    {
        public RoomType Type; 
        
        public RoomNode(RoomType type)
        {
            Type = type;
        }
    }
}