using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{

    
    /// <summary>
    /// <para>Подбирает и создаёт комнаты по сиду и собирает их в кластеры.<br/>
    /// Распределяет обязательные и случайные комнаты по группам с соблюдением квот по комнатам и соблюдении определённых параметров.<br/>
    /// </para>
    /// Используется в <see cref="LevelOrchestrator"/>
    /// </summary>
    public class LevelGenerator
    {
        private readonly Random _random;
        private readonly LevelConfig _config;
        private readonly int _targetRoomCount;
        private int nextNodeId = 1;
        private int nextClusterId = 1;
        
        private readonly Dictionary<RoomType, int> _spawnedRoomCounts = new();

        /// <summary>
        /// Рассчитывается целевое количество комнат на уровне(<see cref="_targetRoomCount"/>) выбирается случайное число между минимальным и максимальным значениями из конфигурации.<br/>
        /// Жёсткое ограничение в минимум 7 конмат, даже если в конфиге указано меньше
        /// </summary>
        /// <param name="config">ScriptableObject с конфигурацией</param>
        /// <param name="seedOverride">Сид для генерации</param>
        public LevelGenerator(LevelConfig config, int? seedOverride = null)
        {
            _config = config;
            var actualSeed = seedOverride ?? (_config.UseRandomSeed ? new Random().Next() : _config.CustomSeed);
            _random = new Random(actualSeed);
            
            var minRoomsRequired = Math.Max(_config.MinRooms, _config.AbsoluteMinRooms);
            _targetRoomCount = _random.Next(minRoomsRequired, _config.MaxRooms + 1);
        }
        
        /// <summary>
        /// <para>Главный метод запускающий генерацию значений для уровня.<br/>
        /// Создаёт и складывает все <see cref="RoomCluster"/> в список clusters.<br/>
        /// Вызывает запуск генерации базового кластера с хабом(<see cref="BuildCoreCluster"/>).<br/>
        /// Вычисляет общее количество комнат на остальные кластера, вычитая 4 комнаты базового кластера.<br/>
        /// Распределяет по кластерам количество комнатна каждого с помощью <see cref="CalculateClusterSizes"/><br/>
        /// Создаёт список обязательных комнат на уровне с помощью <see cref="BuildMandatoryPool"/>
        /// Для каждого кластера рассчитывается квота обязательных комнат
        /// Обязательные комнаты изымаются из начала пула и добавляются в кластер.
        /// Оставшееся свободное место в кластере заполняется через метод <see cref="GetRandomRepeatableRoom"/>
        /// </para>
        /// </summary>
        /// <returns>Список всех кластеров clusters</returns>
        public List<RoomCluster> GenerateClusters()
        {
            var clusters = new List<RoomCluster>();
            _spawnedRoomCounts.Clear();
            
            nextNodeId = 1;
            nextClusterId = 1;
            
            // 1. Создание ядра
            var coreCluster = BuildCoreCluster();
            clusters.Add(coreCluster);

            var remainingRooms = _targetRoomCount - coreCluster.Rooms.Count;
            var clusterSizes = CalculateClusterSizes(remainingRooms);
            
            // 2. Формирование пула обязательных комнат (гарантированно хотя бы по 1 разу)
            var mandatoryPool = new List<RoomType>(_config.MandatoryRooms);

            for (var i = 0; i < clusterSizes.Count; i++)
            {
                var size = clusterSizes[i];
                var isLastCluster = i == clusterSizes.Count - 1;
                
                var mandatoryToTake = size >= _config.MaxClusterSize ? 2 : 1;

                if (isLastCluster)
                {
                    mandatoryToTake = Math.Max(mandatoryToTake, mandatoryPool.Count);
                }
                else
                {
                    var clustersLeft = clusterSizes.Count - i;
                    if (mandatoryPool.Count > mandatoryToTake + (clustersLeft - 1) * 2)
                    {
                        mandatoryToTake = Math.Min(mandatoryToTake + 1, 2);
                    }
                }

                mandatoryToTake = Math.Min(mandatoryToTake, size);
                mandatoryToTake = Math.Min(mandatoryToTake, mandatoryPool.Count);

                var cluster = new RoomCluster(nextClusterId++);

                // Добавляем обязательные комнаты
                for (var m = 0; m < mandatoryToTake; m++)
                {
                    var roomType = mandatoryPool[0];
                    AddNodeToCluster(cluster, roomType);
                    mandatoryPool.RemoveAt(0);
                }

                // Заполняем оставшееся место в кластере случайными комнатами из разрешенного пула
                var repeatablesToTake = size - cluster.Rooms.Count;
                for (var r = 0; r < repeatablesToTake; r++)
                {
                    var repeatableType = GetValidRepeatableRoom(cluster);
                    AddNodeToCluster(cluster, repeatableType);
                }

                clusters.Add(cluster);
            }

            return clusters;
        }
        
        
        /// <summary>
        /// Базовый кластер в который добавляется заданные комнаты из конфига <see cref="LevelConfig"/>
        /// </summary>
        /// <returns>Возвращает <see cref="RoomCluster"/> с заданными комнатами</returns>
        private RoomCluster BuildCoreCluster()
        {
            var cluster = new RoomCluster(nextClusterId++);

            foreach (var roomType in _config.CoreRooms)
            {
                AddNodeToCluster(cluster, roomType);
            }
            return cluster;
        }
        
        
        private void AddNodeToCluster(RoomCluster cluster, RoomType type)
        {
            cluster.Rooms.Add(new RoomNode(nextNodeId++, type, cluster.Id));
            
            if (_spawnedRoomCounts.ContainsKey(type))
                _spawnedRoomCounts[type]++;
            else
                _spawnedRoomCounts[type] = 1;
        }

        /// <summary>
        /// Метод обёртка для создания новых нод
        /// </summary>
        /// <param name="type">Тип комнаты</param>
        /// <param name="clusterId">ID кластера в котором лежит нода</param>
        /// <returns>Возвращает новую ноду с заданным типом комнаты</returns>
        private RoomNode CreateNode(RoomType type, int clusterId) => new(nextNodeId++, type, clusterId);

        /// <summary>
        /// Генерирует случайное значение от <see cref="MinClusterSize"/> до <see cref="MaxClusterSize"/> включительно<br/>
        /// Если оставшееся количество комнат меньше минимального размера кластера, прибавляет это значение к предыдущему клстеру
        /// </summary>
        /// <param name="remainingRooms">Общее количество комнат</param>
        /// <returns>Список чисел - размеров кластеров</returns>
        private List<int> CalculateClusterSizes(int remainingRooms)
        {
            var sizes = new List<int>();

            while (remainingRooms > 0)
            {
                if (remainingRooms <= _config.MaxClusterSize)
                {
                    sizes.Add(remainingRooms);
                    break;
                }

                var size = _random.Next(_config.MinClusterSize, _config.MaxClusterSize + 1);

                if (remainingRooms - size < _config.MinClusterSize)
                {
                    size = remainingRooms - _config.MinClusterSize;
                }

                sizes.Add(size);
                remainingRooms -= size;
            }

            return sizes;
        }
        
        private RoomType GetValidRepeatableRoom(RoomCluster currentCluster)
        {
            var validCandidates = new List<RoomType>();

            foreach (var setting in _config.ClusterRoomSettings)
            {
                var roomType = setting.roomType;

                // Проверка на множественное число внутри кластера
                if (!setting.allowMultiplePerCluster && currentCluster.Rooms.Exists(r => r.Type == roomType))
                {
                    continue;
                }

                // Проверка глобальных ограничений на уровне (Level Constraints)
                if (ViolatesLevelConstraints(roomType))
                {
                    continue;
                }

                validCandidates.Add(roomType);
            }

            // Если по какой-то причине кандидатов не осталось, берем любой разрешенный тип из пула кластера
            if (validCandidates.Count == 0 && _config.ClusterRoomSettings.Count > 0)
            {
                return _config.ClusterRoomSettings[_random.Next(_config.ClusterRoomSettings.Count)].roomType;
            }

            return validCandidates[_random.Next(validCandidates.Count)];
        }
        
        private bool ViolatesLevelConstraints(RoomType roomType)
        {
            var constraint = _config.LevelConstraints.FirstOrDefault(c => c.roomType == roomType);
            if (constraint.roomType == RoomType.None) return false;

            var currentCount = _spawnedRoomCounts.TryGetValue(roomType, out var count) ? count : 0;

            return constraint.countOperator switch
            {
                RoomCountOperator.LessOrEqual => currentCount >= constraint.targetCount,
                RoomCountOperator.Equal => currentCount >= constraint.targetCount,
                RoomCountOperator.GreaterOrEqual => false, // Превысить или сравняться с >= всегда можно при генерации
                _ => false
            };
        }
    }
}