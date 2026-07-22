namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class LevelGraphConfig
    {
        public int Difficulty { get; set; }
        public int MinRooms { get; set; }
        public int MaxRooms { get; set; }
        public int MaxConnectionsPerNode { get; set; } = 3;
        
        public int MinConnectionsPerNode { get; set; } = 1;
        public int MaxConnectionsPerRoom  { get; set; } = 4;
    }
}