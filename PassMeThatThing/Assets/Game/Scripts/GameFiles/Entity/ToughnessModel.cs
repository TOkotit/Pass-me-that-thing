using System;
using UnityEngine;

namespace Entity
{
    public class ToughnessModel
    {
        private int _currentToughness;
        private int _maxToughness;

        public event Action<int, int> OnToughnessChanged; //currT, maxT
        public event Action OnToughnessBreak;

        public int CurrentToughness => _currentToughness;

        public int MaxToughness => _maxToughness;


        public void ReduceToughness(int amount)
        {
            if (amount == 0) return;
            _currentToughness -= amount;
            _currentToughness = Mathf.Clamp(_currentToughness, 0, _maxToughness);
            // Debug.Log($"_currentToughness : {_currentToughness}");

            OnToughnessChanged?.Invoke(_currentToughness, _maxToughness);
            if (_currentToughness <= 0) OnToughnessBreak?.Invoke();
        }

        public void SetToughness(int newValue)
        {
            _currentToughness = Mathf.Clamp(newValue, 0, _maxToughness);
            
            OnToughnessChanged?.Invoke(_currentToughness, _maxToughness);
            if (_currentToughness <= 0) OnToughnessBreak?.Invoke();
        }

        public void SetMaxToughness(int newMaxToughness, bool fullToughness)
        {
            _maxToughness = newMaxToughness;
            if (newMaxToughness < _currentToughness || fullToughness ) 
                SetToughness(newMaxToughness);
        }
    }
}