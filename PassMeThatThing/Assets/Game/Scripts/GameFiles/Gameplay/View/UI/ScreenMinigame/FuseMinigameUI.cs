using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using Game.Gameplay.View.UI.ScreenMinigame;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FuseMinigameUI : MinigameUI
{
    [SerializeField] private List<Toggle> toggles;
    [SerializeField] private TextMeshProUGUI countText;

    private int _counter;
    private float _goal;


    public event Action<int> OnCounterChanged;
    public int Counter
    {
        get => _counter;
        set
        {
            OnCounterChanged?.Invoke(value);
            _counter = value;
        }
    }

    private void Start()
    {
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        OnCounterChanged += UpdateCountText;
        UpdateCountText(0);
    }

    private void OnDestroy()
    {
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
        
        OnCounterChanged -= UpdateCountText;
    }

    public void OnToggleValueChanged(bool value)
    {
        if (value)
        {
            Counter++;
        }
        else
        {
            Counter--;
        }

        if (Counter == toggles.Count)
        {
            _isCompleted  = true;
            mainBinder.CompleteMinigame();
        }
    }

    public void UpdateCountText(int count)
    {
        countText.text = $"{count} / {toggles.Count}";
        
    }
}
