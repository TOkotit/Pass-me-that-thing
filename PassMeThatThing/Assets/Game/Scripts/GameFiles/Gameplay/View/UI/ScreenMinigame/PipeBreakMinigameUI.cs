using System;
using UnityEngine;
using UnityEngine.UI;

public class PipeBreakMinigameUI : MinigameUI
{
    [SerializeField] private Button minigameBtn;

    private int _counter;
    
    private void Start()
    {
        minigameBtn.onClick.AddListener(OnMGBtnClick);
    }

    private void OnDestroy()
    {
        minigameBtn.onClick.RemoveListener(OnMGBtnClick);
    }

    public void OnMGBtnClick()
    {
        _counter++;
        if (_counter >= 3)
        {
            mainBinder.CompleteMinigame();
        }
    }
}
