using System;
using System.Collections;
using FishNet.Object;

using UnityEngine;


namespace Game.Scripts.Utils
{
    public class NetworkTimer
    {
        private float _time;
        private float _remainingTime;
        private Coroutine _countdownCoroutine;
        private NetworkBehaviour _context;
        private event Action<float> _onTick;
        
        public event Action TimeIsOver;

        public NetworkTimer(NetworkBehaviour context, Action<float> onTickCallback)
        {
            _context = context;
            _onTick = onTickCallback;
        }
        
        public void Set(float time)
        {
            _time = time;
            _remainingTime = _time;
        }

        public void Start()
        {
            if (!_context.IsServerStarted)
            {
                return;
            }
            
            Stop();
            _countdownCoroutine = _context.StartCoroutine(Countdown());
        }
        public void Stop()
        {
            if (_countdownCoroutine != null)
            {
                _context.StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }
        }

        private IEnumerator Countdown()
        {
            while (_remainingTime > 0)
            {
                _remainingTime -= Time.deltaTime;
                _onTick?.Invoke(_remainingTime);
                yield return null;
            }
            _remainingTime = 0;
            _onTick?.Invoke(0);
            TimeIsOver?.Invoke();
        }
    }
}