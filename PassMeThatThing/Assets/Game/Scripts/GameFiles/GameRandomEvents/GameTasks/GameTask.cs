using Game.Scripts.Enums;
using Game.Scripts.GameFiles.GameRandomEvents;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks
{
    public struct GameTaskParameters
    {
        public int cost;
        public float timeLimit;

        public GameTaskType gameTaskType;
        public string taskField; //тип ивента или id предмета

        public GameTaskStructureType structType;
        public CompositeTaskStepData[] steps;
    }

    public struct GameTaskData
    {
        public int gameTaskId;
        public bool isTaskOverdue;
        public int cost;

        public string message;

        public GameTaskStructureType structType;
        public CompositeTaskStepData[] steps;

        public GameTaskType gameTaskType;
        public string taskField; //тип ивента или id предмета
    }

    public struct CompositeTaskStepData
    {
        public string message;
        public bool isComplete;

        public GameTaskType gameTaskType;
        public string taskField;
    }

    // только таймеры
    // синхронизируется только 1 раз при добавлении и удалении
    [Serializable]
    public class GameTask 
    {
        public int gameTaskId;

        public float timeLimit;
        public float time;

        public event Action<int> OnTaskTimerEnd; //taskId
        public event Action<float, float> OnTaskTimerTicked;

        public void InitTask(float timeLimit)
        {
            time = 0;
            this.timeLimit = timeLimit;
        }

        public void DestroyTask() { }

        public void TickTimer()
        {
            time++;
            OnTimerTicked();
            if (time >= timeLimit)
            {
                OnTimerFinished();
            }
        }

        private void OnTimerFinished()
        {
            OnTaskTimerEnd?.Invoke(gameTaskId);
        }

        private void OnTimerTicked()
        {
            OnTaskTimerTicked?.Invoke(time, timeLimit);
        }
    }
}
