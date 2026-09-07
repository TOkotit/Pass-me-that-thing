namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    /// <summary>
    /// <para>Класс хранящий и задающий входные данные конфигурации длф генератора<br/>
    /// Хранит уровень сложности, минимальное и максимальное количество комнат, а также допустимое количество соединений для одной комнаты<br/></para>
    /// Используется в <see cref="LevelGenerator"/>
    /// </summary>
    public class LevelConfig
    {
        public int Difficulty { get; set; }
        public int MinRooms { get; set; } = 10;
        public int MaxRooms { get; set; } = 30;
        public int MaxConnectionsPerRoom  { get; set; } = 4;
    }
}