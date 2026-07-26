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
            
            var farthestPoint = FindFarthestNode(root);
            var hangar = CreateNode(RoomTypeNew.RecoveryHangar);
            var branchId = _nodeBranchIds.GetValueOrDefault(farthestPoint, 0);
            
            AttachNodeWithOptionalTunnel(farthestPoint, hangar, branchId, false);
            
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

            AttachCoreRoom(commandCenter, generator, 1);
            AttachCoreRoom(commandCenter, warehouse, 2);
            AttachCoreRoom(commandCenter, livingBlock, 3);

            OpenNodes.Add(generator);
            OpenNodes.Add(warehouse);
            OpenNodes.Add(livingBlock);

            if (commandCenter.ConnectedNodes.Count < _config.MaxConnectionsPerRoom)
            {
                OpenNodes.Add(commandCenter);
            }

            return commandCenter;

        }
        private void AttachCoreRoom(RoomNodeNew hub, RoomNodeNew room, int branchId)
        {
            AttachNodeWithOptionalTunnel(hub, room, branchId, false);
        }

        private RoomNodeNew CreateNode(RoomTypeNew type)
        {
            var newNode = new RoomNodeNew(_nextNodeId++, type);
            AllNodes.Add(newNode);
            return newNode;
        } 
        
        private void AttachNodeWithOptionalTunnel(RoomNodeNew parent, RoomNodeNew child, int branchId, bool updateOpenNodes)
        {
            if (_random.Next(100) < 15)
            {
                var tunnel = CreateNode(RoomTypeNew.TechnicalTunnels);
                
                parent.Connect(tunnel);
                tunnel.Connect(child);

                _parents[tunnel] = parent;
                _parents[child] = tunnel;

                _nodeBranchIds[tunnel] = branchId;
                _nodeBranchIds[child] = branchId;

                if (updateOpenNodes)
                {
                    if (parent.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom)
                        OpenNodes.Remove(parent);
                    OpenNodes.Add(child);
                }
            }
            else
            {
                parent.Connect(child);
                _parents[child] = parent;
                _nodeBranchIds[child] = branchId;

                if (updateOpenNodes)
                {
                    if (parent.ConnectedNodes.Count >= _config.MaxConnectionsPerRoom)
                        OpenNodes.Remove(parent);
                    OpenNodes.Add(child);
                }
            }
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
            var parentNode = candidates[_random.Next(candidates.Count)];
            var newNode = CreateNode(roomType);
            var parentBranch = _nodeBranchIds.GetValueOrDefault(parentNode, 0);
            
            AttachNodeWithOptionalTunnel(parentNode, newNode, parentBranch, true);
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
                
                if (current.Type != RoomTypeNew.TechnicalTunnels && currentDist > maxDistance)
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