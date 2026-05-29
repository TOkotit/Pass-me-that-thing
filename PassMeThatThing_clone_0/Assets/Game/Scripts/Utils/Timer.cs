using System;
using System.Collections;
using UnityEngine;

namespace Systems
{
    
    // почти то же самое, что и Coroutines, только работает через подвязывание к монобеху на сцене, не является сам монобехом
    // и может вызываться из нативных c# классов
    public class Timer
    {
        private float _time;
        private float _remainigTime;

        private IEnumerator _countdown;

        private MonoBehaviour _context;

        public event Action<float> HasBeenUpdated;
        public event Action TimeIsOver;

        public Timer(MonoBehaviour context) => _context = context;

        public void Set(float time)
        {
            _time = time;
            _remainigTime = _time;
        }

        public void StartCountingTime()
        {
            _countdown = Countdown();
            _context.StartCoroutine(_countdown);
        }

        public void StopCountingTime()
        {
            if (_countdown != null)
                _context.StopCoroutine(_countdown);
        }

        private IEnumerator Countdown()
        {
            while (_remainigTime >= 0)
            {
                _remainigTime -= Time.deltaTime;

                HasBeenUpdated?.Invoke(_remainigTime / _time);

                yield return null;
            }
            TimeIsOver?.Invoke();
        }
    }
}