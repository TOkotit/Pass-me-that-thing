using System;
using System.Collections.Generic;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class LevelGraphBuilderNew
    {
        private Random _random = new();
        private List<RoomNodeNew> AllNodes { get;  set; } = new();
        private List<RoomNodeNew> OpenNodes  { get;  set; } = new();
        private int _nextNodeId = 0;
        
        private readonly LevelGraphConfig _config;
        private readonly int _targetRoomCount;

        public LevelGraphBuilderNew(LevelGraphConfig config)
        {
            _config = config;
            _targetRoomCount = _random.Next(config.MinRooms, config.MaxRooms + 1);
        }
        
        
        public LevelGraphResult GenerateGraph()
        {
            AllNodes.Clear();
            OpenNodes.Clear();
            _nextNodeId = 0;
            
            var root = BuildCore();
            
            var medicalRoom = CreateNode(RoomTypeNew.MedicalBlock);
            var room = OpenNodes[_random.Next(OpenNodes.Count)];
            room.Connect(medicalRoom);
            if (room.ConnectedNodes.Count == _config.MaxConnectionsPerRoom)
                OpenNodes.Remove(room);
            OpenNodes.Add(medicalRoom);
            var roomsLeftToGenerate = _targetRoomCount - AllNodes.Count;

            
            return new LevelGraphResult
            {
                Root = root,
                AllNodes = new List<RoomNodeNew>(AllNodes),
                Difficulty = 1,
                IsValid = true
            };
        }
        
        
        private RoomNodeNew BuildCore()
        {
            var commandCenter = CreateNode(RoomTypeNew.CommandCenter);

            var warehouse = CreateNode(RoomTypeNew.Warehouse);
            var generator = CreateNode(RoomTypeNew.Generator);
            var livingBlock = CreateNode(RoomTypeNew.LivingBlock);

            commandCenter.Connect(warehouse);
            commandCenter.Connect(generator);
            commandCenter.Connect(livingBlock);
            
            OpenNodes.Add(generator);
            OpenNodes.Add(warehouse);
            OpenNodes.Add(livingBlock);
            
            return commandCenter;
        }
        

        private RoomNodeNew CreateNode(RoomTypeNew type)
        {
            var newNode = new RoomNodeNew(_nextNodeId++, type);
            AllNodes.Add(newNode);
            return newNode;
        } 
    }
}