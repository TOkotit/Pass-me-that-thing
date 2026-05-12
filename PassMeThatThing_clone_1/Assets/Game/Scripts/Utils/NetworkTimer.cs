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
        private IEnumerator _countdown;
        private NetworkBehaviour _context;
        private Action<float> _onTick;
        
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
            if (!_context.isServer) return;
            
            Stop();
            _countdown = Countdown();
            _context.StartCoroutine(_countdown);
        }
        public void Stop()
        {
            if (_countdown != null)
                _context.StopCoroutine(_countdown);
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