using System;
using System.Globalization;
using DG.Tweening;
using Game.Gameplay.View.UI.ScreenMinigame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PipeBreakMinigameUI : MinigameUI
{
    [SerializeField] private Spinner nut;
    
    [SerializeField] private Slider goalSlider;

    private int _counter;
    private float _goal;

    private bool _isCompleted;
    public event Action<float> OnGoalChanged;

    public float Goal
    {
        get => _goal;
        set
        {
            _goal = Math.Clamp(value, 0f, 1f);
            OnGoalChanged?.Invoke(_goal);
        }
    }


    private void Start()
    {
        nut.OnValueDeltaChanged += AddToGoal;
        
        OnGoalChanged += UpdateGoal;
    }

    private void OnDestroy()
    {
        nut.OnValueDeltaChanged -= AddToGoal;
        
        OnGoalChanged -= UpdateGoal;
    }

    public void AddToGoal(float value)
    {
        Goal += -value / 10000;
        if (_isCompleted) return;
        if (Goal >= 1)
        {
            _isCompleted =  true;
            mainBinder.CompleteMinigame();
        }
    }

    public void UpdateGoal(float value)
    {
        goalSlider.value = value;
    }
}
