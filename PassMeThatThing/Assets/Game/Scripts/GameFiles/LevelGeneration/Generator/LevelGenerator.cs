using System;
using System.Collections.Generic;
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
        private int nextNodeId = 0;
        private int nextClusterId = 0;
        private int _medicalBlockCount;

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
            _medicalBlockCount = 0;
            
            nextNodeId = 1;
            nextClusterId = 1;
            
            var coreCluster = BuildCoreCluster();
            clusters.Add(coreCluster);

            var remainingRooms = _targetRoomCount - coreCluster.Rooms.Count;
            var clusterSizes = CalculateClusterSizes(remainingRooms);
            var mandatoryPool = BuildMandatoryPool();

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

                for (var m = 0; m < mandatoryToTake; m++)
                {
                    var node = CreateNode(mandatoryPool[0], cluster.Id);
                    cluster.AddRoom(node);
                    mandatoryPool.RemoveAt(0);
                }

                var repeatablesToTake = size - cluster.Rooms.Count;
                for (var r = 0; r < repeatablesToTake; r++)
                {
                    cluster.Rooms.Add(CreateNode(GetRandomRepeatableRoom(cluster), cluster.Id));
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
                var node = CreateNode(roomType, cluster.Id);
                cluster.AddRoom(node);
            }
            return cluster;
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
        
        /// <summary>
        /// Создаёт список комнат, которые обязательно должны появиться на уровне
        /// В зависимости от общего размера уровня добавляются ивентовые комнаты
        /// </summary>
        /// <returns>Список типов комнат</returns>
        private List<RoomType> BuildMandatoryPool()
        {
            var pool = new List<RoomType>(_config.MandatoryRooms);

            if (_medicalBlockCount < _config.MaxMedicalBlocksOnLevel && pool.Contains(RoomType.MedicalBlock))
            {
                _medicalBlockCount++;
            }

            var events = new List<RoomType>(_config.EventRooms);

            if (events.Count > 0)
            {
                if (_targetRoomCount >= _config.HighEventThreshold)
                {
                    pool.AddRange(events);
                }
                else if (_targetRoomCount >= _config.LowEventThreshold)
                {
                    pool.Add(events[_random.Next(events.Count)]);
                }
            }

            ShuffleList(pool);
            return pool;
        }

        /// <summary>
        /// Использует список с типами комнат дубликатами.<br/>
        /// Проводятся проверки состава текущего кластера. <see cref="RoomType.Workshop"/> и <see cref="RoomType.Armory"/> добавляются в список возможных вариантов только если их еще нет в этом конкретном кластере.<br/>
        /// <see cref="RoomType.MedicalBlock"/> добавляется в список возможных вариантов только если его нет в текущем кластере и общее количество медицинских блоков на уровне меньше 2.<br/>
        /// Из сформированного списка выбирается случайная комната
        /// </summary>
        /// <param name="currentCluster">Кластер</param>
        /// <returns>Тип комнаты для заполнения кластера</returns>
        private RoomType GetRandomRepeatableRoom(RoomCluster currentCluster)
        {
            var types = new List<RoomType>(_config.BaseRepeatableRooms);

            if (!currentCluster.Rooms.Exists(r => r.Type == RoomType.Workshop))
            {
                types.Add(RoomType.Workshop);
            }

            if (!currentCluster.Rooms.Exists(r => r.Type == RoomType.Armory))
            {
                types.Add(RoomType.Armory);
            }

            if (_medicalBlockCount < _config.MaxMedicalBlocksOnLevel && !currentCluster.Rooms.Exists(r => r.Type == RoomType.MedicalBlock))
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
        
        
        /// <summary>
        /// Перемешивает случайным образом список
        /// </summary>
        /// <param name="list">Список элементов</param>
        /// <typeparam name="T">Элемент</typeparam>
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