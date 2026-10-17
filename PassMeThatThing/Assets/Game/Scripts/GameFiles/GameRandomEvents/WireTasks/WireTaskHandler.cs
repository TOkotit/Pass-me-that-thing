using Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks;

using Assets.Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.Enums;

using Mirror;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using VContainer;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using R3;

namespace Assets.Game.Scripts.GameFiles.GameRandomEvents.WireTasks
{
    public class WireTaskHandler : NetworkBehaviour
    {
        //[SerializeField] private ItemTaskDepotPointView view;
        [SerializeField] private GameTaskHandler taskHandler;
        [SerializeField] private float taskTimeLimit = 30;
        [SerializeField] private Outline outlineComponent;

        [SerializeField] private List<WireNodePort> ports;

        [Inject] private GlobalStageDatabase _globalStageDatabase;

        private int _wireTaskHandlerId;
        private GameTask _currentTask;

        [SyncVar(hook = nameof(OnHasActiveTaskChanged))]
        private bool _hasActiveTask;

        private CompositeDisposable _portSubs = new();


        [Server]
        public void CreateWireTask()
        {
            if (_currentTask != null) return;

            var p = new GameTaskParameters();

            p.structType = GameTaskStructureType.CompositeParent;
            p.gameTaskType = GameTaskType.Wire;
            p.timeLimit = taskTimeLimit;
            p.taskField = "";
            p.cost = _globalStageDatabase.BaseQuotaConst;
            p.steps = new CompositeTaskStepData[ports.Count];

            for (var i = 0; i < ports.Count; i++)
            {
                p.steps[i] = new CompositeTaskStepData() { gameTaskType = GameTaskType.Wire, message = $"{i + 1}" };

                var index = i;

                _portSubs.Add(
                    ports[i].OnWireStateChanged.Subscribe(newValue =>
                    {
                        Debug.Log("[GameTasks] port Subscribe");
                        SetCompleteStep(newValue.Item2, index);
                    }));
            }

            _currentTask = taskHandler.GameTasksManager.CreateGameTask(p);

            _currentTask.OnTaskTimerEnd += OnTaskTimerEnd;
        }

        private void SetCompleteStep(bool isOn, int stepIndex)
        {
            taskHandler.GameTasksManager
                .SetCompleteTaskStep(_currentTask.gameTaskId, stepIndex, isOn, CompleteAndDestroyItemTask);
        }

        private void OnTaskTimerEnd(int taskId)
        {
            _currentTask.OnTaskTimerEnd -= OnTaskTimerEnd;
            _portSubs.Clear();

            taskHandler.GameTasksManager.DestroyGameTask(taskId);

            _currentTask = null;
        }

        [Server]
        public void CompleteAndDestroyItemTask()
        {
            _currentTask.OnTaskTimerEnd -= OnTaskTimerEnd;
            _portSubs.Clear();

            taskHandler.GameTasksManager.CompleteAndDestroyGameTask(_currentTask.gameTaskId);

            _currentTask = null;
        }

        [Server]
        private void UpdateOutlineState()
        {
           
        }

        private void OnHasActiveTaskChanged(bool oldValue, bool newValue)
        {
            if (outlineComponent)
            {
                outlineComponent.enabled = newValue;
            }
        }

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            RegisterWireTaskHandler();
        }

        [Server]
        public override void OnStopServer()
        {
            base.OnStartServer();
            UnRegisterWireTaskHandler();
        }

        [Server]
        private void RegisterWireTaskHandler()
        {
            if (taskHandler.GameTasksManager != null)
            {
                _wireTaskHandlerId = taskHandler.GameTasksManager.RegisterSceneWireTaskHandler(this);
            }
        }

        [Server]
        private void UnRegisterWireTaskHandler()
        {
            if (taskHandler.GameTasksManager != null)
            {
                taskHandler.GameTasksManager.UnregisterSceneWireTaskHandler(_wireTaskHandlerId);
            }
        }
    }
}
