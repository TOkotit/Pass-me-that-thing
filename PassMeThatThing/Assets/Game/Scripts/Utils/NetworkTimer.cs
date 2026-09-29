using System;
using System.Collections;
using Mirror;
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

        private WaitForSeconds _secDelay = new WaitForSeconds(1);
        
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
            if (!_context.isServer)
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
                _remainingTime -= 1;
                _onTick?.Invoke(_remainingTime);
                yield return _secDelay;
            }
            _remainingTime = 0;
            _onTick?.Invoke(0);
            TimeIsOver?.Invoke();
        }
    }
}