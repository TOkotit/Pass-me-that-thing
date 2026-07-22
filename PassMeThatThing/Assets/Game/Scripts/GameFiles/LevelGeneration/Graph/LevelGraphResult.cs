using System.Collections.Generic;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class LevelGraphResult
    {
        public RoomNodeNew Root { get; set; }         
        public List<RoomNodeNew> AllNodes { get; set; } 
        public int Difficulty { get; set; }             
        public bool IsValid { get; set; }
    }
}