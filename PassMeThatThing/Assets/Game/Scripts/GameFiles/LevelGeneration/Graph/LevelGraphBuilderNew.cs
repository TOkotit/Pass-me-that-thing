using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class LevelGraphBuilderNew
    {
        private readonly Random _random = new();
        private List<RoomNodeNew> AllNodes { get;  set; } = new();
        private List<RoomNodeNew> OpenNodes  { get;  set; } = new();
        private readonly Dictionary<RoomNodeNew, RoomNodeNew> _parents = new();
        
        
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
            var slotsForPool = _targetRoomCount - 5;
            
            
            var roomPool = BuildRoomPool(slotsForPool);

            foreach (var roomType in roomPool)
            {
                AttachRoomToGraph(roomType);
            }
            
            var farthestPoint = FindFarthestNode(root);

            var hangar = CreateNode(RoomTypeNew.RecoveryHangar);
            farthestPoint.Connect(hangar);
            
            CreateCycles();
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
            
            _parents[warehouse] = commandCenter;
            _parents[generator] = commandCenter;
            _parents[livingBlock] = commandCenter;
            
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
        
        
        private List<RoomTypeNew> BuildRoomPool(int slotsLeft)
        {
            var pool = new List<RoomTypeNew>
            {
                RoomTypeNew.MedicalBlock,
            };

            slotsLeft -= 1;

            var optionalTypes = new List<RoomTypeNew>
            {
                RoomTypeNew.Laboratory,
                RoomTypeNew.Workshop,
                RoomTypeNew.Server,
                RoomTypeNew.WaterPurification,
                RoomTypeNew.Armory
            };

            ShuffleList(optionalTypes);

            var index = 0;
            while (slotsLeft > 0)
            {
                pool.Add(optionalTypes[index % optionalTypes.Count]);
                index++;
                slotsLeft--;
            }

            ShuffleList(pool);

            return pool;
        }

        private void AttachRoomToGraph(RoomTypeNew roomType)
        {
            if (OpenNodes.Count == 0) return;

            var parentNode = OpenNodes[_random.Next(OpenNodes.Count)];
            var newNode = CreateNode(roomType);

            parentNode.Connect(newNode);
            _parents[newNode] = parentNode;
            if (parentNode.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom)
            {
                OpenNodes.Remove(parentNode);
            }

            OpenNodes.Add(newNode);
        }
        
        private void CreateCycles()
        {
            var candidatePairs = new List<(RoomNodeNew nodeA, RoomNodeNew nodeB)>();

            for (var i = 0; i < OpenNodes.Count; i++)
            {
                for (var j = i + 1; j < OpenNodes.Count; j++)
                {
                    var a = OpenNodes[i];
                    var b = OpenNodes[j];

                    if (a.ConnectedNodes.Contains(b)) continue;

                    _parents.TryGetValue(a, out var parentA);
                    _parents.TryGetValue(b, out var parentB);

                    var grandParentA = parentA != null && _parents.TryGetValue(parentA, out var ga) ? ga : null;
                    var grandParentB = parentB != null && _parents.TryGetValue(parentB, out var gb) ? gb : null;

                    var shareParent = parentA != null && parentA == parentB;             
                    var shareGrandparent = grandParentA != null && grandParentA == grandParentB; 
                    var uncleNephew1 = grandParentA != null && grandParentA == parentB;         
                    var uncleNephew2 = parentA != null && parentA == grandParentB;          

                    if (shareParent || shareGrandparent || uncleNephew1 || uncleNephew2)
                    {
                        candidatePairs.Add((a, b));
                    }
                }
            }

            ShuffleList(candidatePairs);

            var targetExtraEdges = _targetRoomCount / 2;

            foreach (var (a, b) in candidatePairs)
            {
                if (targetExtraEdges <= 0) break;

                if (a.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom ||
                    b.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom) continue;
                a.Connect(b);
                targetExtraEdges--;
            }

            OpenNodes.RemoveAll(node => node.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom);
        }
        
        private void ShuffleList<T>(List<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        private static RoomNodeNew FindFarthestNode(RoomNodeNew root)
        {
            var distances = new Dictionary<RoomNodeNew, int>();
            var queue = new Queue<RoomNodeNew>();

            queue.Enqueue(root);
            distances[root] = 0;

            var farthestNode = root;
            var maxDistance = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentDist = distances[current];
                
                if (currentDist > maxDistance)
                {
                    maxDistance = currentDist;
                    farthestNode = current;
                }

                foreach (var neighbor in current.ConnectedNodes.Where(neighbor => !distances.ContainsKey(neighbor)))
                {
                    distances[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                }
            }

            return farthestNode;
        }
    }
}