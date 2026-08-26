using System;
using System.Collections.Generic;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    
    public class RoomCluster
    {
        public List<RoomNode> Rooms { get; set; } = new();
    }
    
    public class LevelGenerator
    {
        private readonly Random _random;
        private readonly LevelGraphConfig _config;
        private readonly int _targetRoomCount;
        
        private int _nextNodeId = 0;
        private int _medicalBlockCount = 0;

        public LevelGenerator(LevelGraphConfig config, int seed)
        {
            _config = config;
            _random = new Random(seed);
            
            var minRoomsRequired = Math.Max(config.MinRooms, 7);
            _targetRoomCount = _random.Next(minRoomsRequired, config.MaxRooms + 1);
        }
        
        public List<RoomCluster> GenerateClusters()
        {
            var clusters = new List<RoomCluster>();
            _nextNodeId = 0;
            _medicalBlockCount = 0;

            var coreCluster = BuildCoreCluster();
            clusters.Add(coreCluster);

            var remainingRooms = _targetRoomCount - coreCluster.Rooms.Count;
            var clusterSizes = CalculateClusterSizes(remainingRooms);
            var mandatoryPool = BuildMandatoryPool();

            for (var i = 0; i < clusterSizes.Count; i++)
            {
                var size = clusterSizes[i];
                var isLastCluster = i == clusterSizes.Count - 1;
                
                var mandatoryToTake = size >= 4 ? 2 : 1;

                if (isLastCluster)
                {
                    mandatoryToTake = Math.Max(mandatoryToTake, mandatoryPool.Count);
                }
                
                if (!isLastCluster)
                {
                    var clustersLeft = clusterSizes.Count - i;
                    if (mandatoryPool.Count > mandatoryToTake + (clustersLeft - 1) * 2)
                    {
                        mandatoryToTake = Math.Min(mandatoryToTake + 1, 2);
                    }
                }

                mandatoryToTake = Math.Min(mandatoryToTake, size);
                mandatoryToTake = Math.Min(mandatoryToTake, mandatoryPool.Count);

                var cluster = new RoomCluster();

                for (var m = 0; m < mandatoryToTake; m++)
                {
                    cluster.Rooms.Add(CreateNode(mandatoryPool[0]));
                    mandatoryPool.RemoveAt(0);
                }

                var repeatablesToTake = size - cluster.Rooms.Count;
                for (var r = 0; r < repeatablesToTake; r++)
                {
                    cluster.Rooms.Add(CreateNode(GetRandomRepeatableRoom(cluster)));
                }

                clusters.Add(cluster);
            }

            return clusters;
        }
        
        private RoomCluster BuildCoreCluster()
        {
            var cluster = new RoomCluster();

            cluster.Rooms.Add(CreateNode(RoomType.CommandCenter));
            cluster.Rooms.Add(CreateNode(RoomType.Generator));
            cluster.Rooms.Add(CreateNode(RoomType.Warehouse));
            cluster.Rooms.Add(CreateNode(RoomType.LivingBlock));

            return cluster;
        }
        private RoomNode CreateNode(RoomType type)
        {
            return new RoomNode(_nextNodeId++, type);
        }

        private const int MinClusterSize = 2;
        private const int MaxClusterSize = 4;

        private List<int> CalculateClusterSizes(int remainingRooms)
        {
            var sizes = new List<int>();

            while (remainingRooms > 0)
            {
                if (remainingRooms <= MaxClusterSize)
                {
                    sizes.Add(remainingRooms);
                    break;
                }

                var size = _random.Next(MinClusterSize, MaxClusterSize + 1);

                if (remainingRooms - size < MinClusterSize)
                {
                    size = remainingRooms - MinClusterSize;
                }

                sizes.Add(size);
                remainingRooms -= size;
            }

            return sizes;
        }
        
        private List<RoomType> BuildMandatoryPool()
        {
            var pool = new List<RoomType>
            {
                RoomType.Laboratory,
                RoomType.Armory,
                RoomType.Workshop,
                RoomType.MedicalBlock
            };

            if (_medicalBlockCount < 2) _medicalBlockCount++;

            var events = new List<RoomType>
            {
                RoomType.Server,
                RoomType.WaterPurification
            };

            if (_targetRoomCount >= 20)
            {
                pool.AddRange(events);
            }
            else if (_targetRoomCount >= 15)
            {
                pool.Add(events[_random.Next(events.Count)]);
            }

            ShuffleList(pool);
            return pool;
        }

        private RoomType GetRandomRepeatableRoom(RoomCluster currentCluster)
        {
            var types = new List<RoomType>
            {
                RoomType.LivingBlock, RoomType.LivingBlock,
                RoomType.Warehouse, RoomType.Warehouse
            };

            if (!currentCluster.Rooms.Exists(r => r.Type == RoomType.Workshop))
            {
                types.Add(RoomType.Workshop);
            }

            if (!currentCluster.Rooms.Exists(r => r.Type == RoomType.Armory))
            {
                types.Add(RoomType.Armory);
            }

            if (_medicalBlockCount < 2 && !currentCluster.Rooms.Exists(r => r.Type == RoomType.MedicalBlock))
            {
                types.Add(RoomType.MedicalBlock);
            }

            var selected = types[_random.Next(types.Count)];

            if (selected == RoomType.MedicalBlock)
            {
                _medicalBlockCount++;
            }

            return selected;
        }
        
        private void ShuffleList<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
    
    
}