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
        private readonly Dictionary<RoomNodeNew, int> _nodeBranchIds = new();
        
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
            if (slotsForPool < 1) slotsForPool = 1;
            
            
            var roomPool = BuildRoomPool(slotsForPool);

            foreach (var roomType in roomPool)
            {
                AttachRoomToGraph(roomType);
            }
            
            CreateCycles();
            AddExtraTechnicalTunnels();
            
            var farthestPoint = FindFarthestNode(root);
            var hangar = CreateNode(RoomTypeNew.RecoveryHangar);
            farthestPoint.Connect(hangar);
            _parents[hangar] = farthestPoint;
            
            return new LevelGraphResult
            {
                Root = root,
                AllNodes = new List<RoomNodeNew>(AllNodes),
                Difficulty = _config.Difficulty,
                IsValid = true
            };
        }
        
        
        private RoomNodeNew BuildCore()
        {
            var commandCenter = CreateNode(RoomTypeNew.CommandCenter);
            _nodeBranchIds[commandCenter] = 0;

            var generator = CreateNode(RoomTypeNew.Generator);
            var warehouse = CreateNode(RoomTypeNew.Warehouse);
            var livingBlock = CreateNode(RoomTypeNew.LivingBlock);

            commandCenter.Connect(generator);
            commandCenter.Connect(warehouse);
            commandCenter.Connect(livingBlock);

            _parents[generator] = commandCenter;
            _parents[warehouse] = commandCenter;
            _parents[livingBlock] = commandCenter;

            _nodeBranchIds[generator] = 1;
            _nodeBranchIds[warehouse] = 2;
            _nodeBranchIds[livingBlock] = 3;
            
            OpenNodes.Add(generator);
            OpenNodes.Add(warehouse);
            OpenNodes.Add(livingBlock);

            if (commandCenter.ConnectedNodes.Count < _config.MaxConnectionsPerRoom)
            {
                OpenNodes.Add(commandCenter);
            }

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
            var pool = new List<RoomTypeNew>();

            pool.Add(RoomTypeNew.Warehouse);
            pool.Add(RoomTypeNew.LivingBlock);
            slotsLeft -= 2;

            var optionalSingles = new List<RoomTypeNew>
            {
                RoomTypeNew.Laboratory,
                RoomTypeNew.Workshop,
                RoomTypeNew.Server
            };
            ShuffleList(optionalSingles);

            var multiples = new List<RoomTypeNew>
            {
                RoomTypeNew.Warehouse,
                RoomTypeNew.LivingBlock,
                RoomTypeNew.WaterPurification,
                RoomTypeNew.Armory
            };

            while (slotsLeft > 0)
            {
                if (optionalSingles.Count > 0 && _random.Next(100) < 30) 
                {
                    pool.Add(optionalSingles[0]);
                    optionalSingles.RemoveAt(0);
                }
                else
                {
                    pool.Add(multiples[_random.Next(multiples.Count)]);
                }
                slotsLeft--;
            }

            ShuffleList(pool);
            return pool;
        }

        private void AttachRoomToGraph(RoomTypeNew roomType)
        {
            if (OpenNodes.Count == 0) return;
            var candidates = OpenNodes;
            if (roomType is RoomTypeNew.Armory or RoomTypeNew.Warehouse)
            {
                var usedBranches = AllNodes
                    .Where(n => n.Type == roomType && _nodeBranchIds.ContainsKey(n))
                    .Select(n => _nodeBranchIds[n])
                    .Distinct()
                    .ToHashSet();

                var separatedCandidates = OpenNodes
                    .Where(n => _nodeBranchIds.ContainsKey(n) && !usedBranches.Contains(_nodeBranchIds[n]))
                    .ToList();

                if (separatedCandidates.Count > 0)
                {
                    candidates = separatedCandidates;
                }
            }
            var parentNode = OpenNodes[_random.Next(OpenNodes.Count)];
            var newNode = CreateNode(roomType);
                
            var parentBranch = _nodeBranchIds.GetValueOrDefault(parentNode, 0);
            
            if (_random.Next(100) < 15)
            {
                var tunnelNode = CreateNode(RoomTypeNew.TechnicalTunnels);
                
                parentNode.Connect(tunnelNode);
                tunnelNode.Connect(newNode);

                _parents[tunnelNode] = parentNode;
                _parents[newNode] = tunnelNode;
                _nodeBranchIds[tunnelNode] = parentBranch;
                _nodeBranchIds[newNode] = parentBranch;

                if (parentNode.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom)
                    OpenNodes.Remove(parentNode);

                OpenNodes.Add(newNode);
            }
            else
            {
                parentNode.Connect(newNode);
                _parents[newNode] = parentNode;
                _nodeBranchIds[newNode] = parentBranch;

                if (parentNode.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom)
                    OpenNodes.Remove(parentNode);

                OpenNodes.Add(newNode);
            }
        }
        private RoomNodeNew SelectParentWeightedByCapacity(List<RoomNodeNew> nodes)
        {
            var weightedList = new List<RoomNodeNew>();
            foreach (var node in nodes)
            {
                var freeSlots = _config.MaxConnectionsPerRoom - node.ConnectedNodes.Count;
                var weight = Math.Max(1, freeSlots * freeSlots);
                for (var i = 0; i < weight; i++)
                {
                    weightedList.Add(node);
                }
            }

            return weightedList[_random.Next(weightedList.Count)];
        }
        
        
        private void CreateCycles()
        {
            var candidatePairs = new List<(RoomNodeNew nodeA, RoomNodeNew nodeB)>();

            for (var i = 0; i < AllNodes.Count; i++)
            {
                for (var j = i + 1; j < AllNodes.Count; j++)
                {
                    var a = AllNodes[i];
                    var b = AllNodes[j];

                    if (a.ConnectedNodes.Contains(b)) continue;
                    if (a.Type == RoomTypeNew.CommandCenter || b.Type == RoomTypeNew.CommandCenter) continue;

                    _parents.TryGetValue(a, out var parentA);
                    _parents.TryGetValue(b, out var parentB);

                    var shareParent = parentA != null && parentA == parentB;

                    if (!shareParent)
                    {
                        candidatePairs.Add((a, b));
                    }
                }
            }

            ShuffleList(candidatePairs);
            var targetRoomsWithAlternativePath = (int)Math.Ceiling(AllNodes.Count * 0.7f);

            foreach (var (a, b) in candidatePairs)
            {
                if (AllNodes.Count(n => n.ConnectedNodes.Count >= 2) >= targetRoomsWithAlternativePath) break;

                if (a.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom ||
                    b.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom) continue;

                var tunnel = CreateNode(RoomTypeNew.TechnicalTunnels);
                a.Connect(tunnel);
                tunnel.Connect(b);

                var branchA = _nodeBranchIds.TryGetValue(a, out var ba) ? ba : 0;
                _nodeBranchIds[tunnel] = branchA;
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
        
        private void AddExtraTechnicalTunnels()
        {
            var availableNodes = OpenNodes.Where(n => n.ConnectedNodes.Count < _config.MaxConnectionsPerRoom).ToList();
            
            var extraTunnelsCount = _random.Next(0, 3);

            while (extraTunnelsCount > 0 && availableNodes.Count > 0)
            {
                var parentNode = availableNodes[_random.Next(availableNodes.Count)];
                
                var tunnel = CreateNode(RoomTypeNew.TechnicalTunnels);
                parentNode.Connect(tunnel);
                
                if (parentNode.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom)
                {
                    availableNodes.Remove(parentNode);
                }

                extraTunnelsCount--;
            }
        }
    }
}