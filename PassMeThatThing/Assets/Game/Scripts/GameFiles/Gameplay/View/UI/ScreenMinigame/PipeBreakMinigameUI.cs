using System;
using System.Globalization;
using DG.Tweening;
using Game.Gameplay.View.UI.ScreenMinigame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PipeBreakMinigameUI : MinigameUI
{
    [SerializeField] private Button minigameBtn;
    
    [SerializeField] private TextMeshProUGUI nutValue;
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
            _goal = value;
            OnGoalChanged?.Invoke(_goal);
        }
    }


    private void Start()
    {
        minigameBtn.onClick.AddListener(OnMGBtnClick);

        nut.OnValueDeltaChanged += UpdateNut;
        nut.OnValueDeltaChanged += AddToGoal;
        
        OnGoalChanged += UpdateGoal;
    }

    private void OnDestroy()
    {
        minigameBtn.onClick.RemoveListener(OnMGBtnClick);
        
        nut.OnValueDeltaChanged -= UpdateNut;
        nut.OnValueDeltaChanged -= AddToGoal;
        
        OnGoalChanged -= UpdateGoal;
    }

    public void OnMGBtnClick()
    {
        // _counter++;
        // if (_counter >= 3)
        // {
        mainBinder.CompleteMinigame();
        // }
    }

    public void UpdateNut(float value)
    {
        nutValue.text = value.ToString(CultureInfo.InvariantCulture);

    }

    public void AddToGoal(float value)
    {
        Goal += value / 10000;
        if (_isCompleted) return;
        if (Goal >= 1)
        {
            minigameBtn.gameObject.SetActive(true);
            minigameBtn.transform.DOScale(1f, 0.2f).From(0f);
            _isCompleted =  true;
        }
    }

    public void UpdateGoal(float value)
    {
        goalSlider.value = value;

        // if (goalSlider.value > 0.5f)
        // {
        //     goalSlider.transform.DOScale(1.5f, 0.5f)
        //         .From(1f)
        //         .SetLoops(1, LoopType.Yoyo)
        //         .SetEase(Ease.OutBounce);
        // }
        
    }
}
