using Game.Scripts.Enums;
using Game.Scripts.GameFiles.GameRandomEvents;
using System;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks
{
    public struct GameTaskParameters
    {
        public GameTaskType gameTaskType;
        public string taskField; //тип ивента или id предмета
        public int cost;

        public float timeLimit;

        public bool canBeOverdue;
    }

    public struct GameTaskData
    {
        public int gameTaskId;

        public bool isTaskOverdue;

        public GameTaskType gameTaskType;
        public string taskField; //тип ивента или id предмета
        public int cost;
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

        public void DestroyTask()
        {

        }

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
