using Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks;
using Assets.Game.Scripts.GameFiles.GameRandomEvents.ItemTasks;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.GameRandomEvents.GameTasks
{
    public class GameTasksManager : NetworkBehaviour
    {
        private WaitForSeconds _waitTasksTick = new WaitForSeconds(1f);

        [Inject] private ItemDatabase _itemDatabase;

        //починка станций, сдача предметов и тд
        private int _idTaskHandlerGenerator = 1;
        private SyncDictionary<int, GameTaskHandler> _gameTaskHandlers = new();

        private Coroutine _timeTickCoroutine;
        private int _idTaskGenerator = 1;
        private SyncDictionary<int, GameTask> _gameTasks = new();
        private SyncDictionary<int, GameTaskData> _gameTasksData = new();

        //терминалы сдачи предметов
        private int _idItemTaskDepotGenerator = 1;
        private readonly SyncDictionary<int, ItemTaskDepotPoint> _itemTasksDepots = new();

        public SyncDictionary<int, GameTask> GameTasks => _gameTasks;
        public SyncDictionary<int, GameTaskData> GameTasksData => _gameTasksData;

        public event Action<int> OnQuotaValueAdded;

        public override void OnStartClient()
        {
            _timeTickCoroutine = StartCoroutine(CalcGameTasksTime());
        }

        public override void OnStopClient()
        {
            StopCoroutine(_timeTickCoroutine);
        }


        //GAME TASK Handlers
        [Server]
        public int RegisterSceneTaskHandler(GameTaskHandler taskHandler)
        {
            var assignedId = _idTaskHandlerGenerator;

            _idTaskHandlerGenerator++;
            Debug.Log($"RegisterSceneTaskHandler, id: {assignedId}");

            _gameTaskHandlers.Add(assignedId, taskHandler);

            return assignedId;
        }

        [Server]
        public void UnregisterTaskHandler(int id)
        {
            if (_gameTaskHandlers.ContainsKey(id))
            {
                _gameTaskHandlers.Remove(id);
            }
        }

        //GAME TASKs

        //просчет таймеров происходит и на сервере и на клиенте
        public IEnumerator CalcGameTasksTime()
        {
            while (true)
            {
                var taskIds = _gameTasks.Keys.ToArray();

                foreach (var taskId in taskIds)
                {
                    if (_gameTasks.TryGetValue(taskId, out var task) &&
                        _gameTasksData.TryGetValue(taskId, out var taskData))
                    {
                        if (!taskData.isTaskOverdue)
                        {
                            task.TickTimer();
                        }
                    }
                }
                yield return _waitTasksTick;
            }
        }

        [Server]
        public GameTask CreateGameTask(GameTaskParameters parameters)
        {
            _idTaskGenerator++;

            var newTask = new GameTask();

            newTask.gameTaskId = _idTaskGenerator;
            newTask.InitTask(parameters.timeLimit);
            newTask.OnTaskTimerEnd += OverdueGameTask;

            var newTaskData = new GameTaskData();

            newTaskData.gameTaskId = _idTaskGenerator;
            newTaskData.gameTaskType = parameters.gameTaskType;
            newTaskData.taskField = parameters.taskField;
            newTaskData.cost = parameters.cost;

            _gameTasks.Add(_idTaskGenerator, newTask);
            _gameTasksData.Add(_idTaskGenerator, newTaskData);

            return newTask;
        }

        [Server]
        public void OverdueGameTask(int taskId)
        {
            if (_gameTasksData.TryGetValue(taskId, out var taskData))
            {
                taskData.isTaskOverdue = true;

                _gameTasksData[taskId] = taskData;
            }
        }

        [Server]
        public void DestroyGameTask(int taskId)
        {
            if (_gameTasks.TryGetValue(taskId, out var task))
            {
                task.OnTaskTimerEnd -= OverdueGameTask;
                task.DestroyTask();
                _gameTasks.Remove(taskId);
            }

            if (_gameTasksData.TryGetValue(taskId, out var taskData))
            {
                _gameTasksData.Remove(taskId);
            }
        }

        [Server]
        public void CompleteAndDestroyGameTask(int taskId)
        {
            if (_gameTasksData.TryGetValue(taskId, out var taskData))
            {
                if (!taskData.isTaskOverdue)
                {
                    AddTaskCostQuota(taskData.cost);
                }
                else
                {
                    AddTaskCostQuota(taskData.cost / 2);
                }
            }

            DestroyGameTask(taskId);
        }

        [Server]
        public void AddTaskCostQuota(int value)
        {
            OnQuotaValueAdded?.Invoke(value);
        }

        //ITEM DEPOT TASKS
        [Server]
        public int RegisterSceneItemTaskDepot(ItemTaskDepotPoint itemTaskDepot)
        {
            var assignedId = _idItemTaskDepotGenerator;

            _idItemTaskDepotGenerator++;
            Debug.Log($"ItemTaskDepot, id: {assignedId}");

            _itemTasksDepots.Add(assignedId, itemTaskDepot);

            return assignedId;
        }

        [Server]
        public void UnregisterSceneItemTaskDepot(int itemTaskDepotId)
        {
            if (_itemTasksDepots.ContainsKey(itemTaskDepotId))
            {
                _itemTasksDepots.Remove(itemTaskDepotId);
            }
        }

        [Server]
        public void TryTriggerRandomItemTasks(int taskCount = 1)
        {
            var depot = _itemTasksDepots.Values.First();
            if (depot == null) return;

            for (var i = 0; i < taskCount; i++)
            {
                var randomItemData = _itemDatabase
                    .TaskItems[Random.Range(0, _itemDatabase.TaskItems.Count)];

                depot.CreateItemTask(randomItemData.Id);
            }
        }

    }
}
